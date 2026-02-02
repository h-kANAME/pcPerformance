using System.Collections.ObjectModel;
using OptimizerApp.Models;
using OptimizerApp.Services;

namespace OptimizerApp.ViewModels;

public sealed class DiskOptimizationViewModel : ObservableObject
{
    private readonly IDiskOptimizationService _diskService;
    private DriveInfo? _selectedDrive;
    private bool _isOperationInProgress = false;
    private string _statusMessage = "Selecciona una unidad para comenzar";
    private bool _hasReport = false;
    private string _reportContent = string.Empty;
    private string _operationButtonContent = "";
    private bool _canExecuteTrim = false;
    private bool _canDefragment = false;

    public ObservableCollection<DriveInfo> AvailableDrives { get; } = new();

    public DriveInfo? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (SetProperty(ref _selectedDrive, value))
            {
                UpdateOperationAvailability();
                
                // Only clear report if switching to a completely different drive
                // Not when just updating the same drive's info
                if (_selectedDrive?.DriveLetter != value?.DriveLetter)
                {
                    HasReport = false;
                }
                
                StatusMessage = value != null 
                    ? $"Unidad seleccionada: {value.DriveLetter} - Disponible: {value.FreeGb:F2} GB / {value.TotalGb:F2} GB"
                    : "Selecciona una unidad para comenzar";
                
                CleanTempCommand?.RaiseCanExecuteChanged();
                ExecuteTrimCommand?.RaiseCanExecuteChanged();
                DefragmentCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsOperationInProgress
    {
        get => _isOperationInProgress;
        private set
        {
            if (SetProperty(ref _isOperationInProgress, value))
            {
                CleanTempCommand?.RaiseCanExecuteChanged();
                ExecuteTrimCommand?.RaiseCanExecuteChanged();
                DefragmentCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool HasReport
    {
        get => _hasReport;
        private set => SetProperty(ref _hasReport, value);
    }

    public string ReportContent
    {
        get => _reportContent;
        private set => SetProperty(ref _reportContent, value);
    }

    public string OperationButtonContent
    {
        get => _operationButtonContent;
        private set => SetProperty(ref _operationButtonContent, value);
    }

    public bool CanExecuteTrim
    {
        get => _canExecuteTrim;
        private set
        {
            if (SetProperty(ref _canExecuteTrim, value))
            {
                ExecuteTrimCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanDefragment
    {
        get => _canDefragment;
        private set
        {
            if (SetProperty(ref _canDefragment, value))
            {
                DefragmentCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand RefreshDrivesCommand { get; }
    public RelayCommand CleanTempCommand { get; }
    public RelayCommand ExecuteTrimCommand { get; }
    public RelayCommand DefragmentCommand { get; }

    public DiskOptimizationViewModel(IDiskOptimizationService diskService)
    {
        _diskService = diskService;

        RefreshDrivesCommand = new RelayCommand(RefreshDrives);
        CleanTempCommand = new RelayCommand(CleanTempFiles, () => !IsOperationInProgress && SelectedDrive != null);
        ExecuteTrimCommand = new RelayCommand(ExecuteTrim, () => !IsOperationInProgress && CanExecuteTrim && SelectedDrive != null);
        DefragmentCommand = new RelayCommand(Defragment, () => !IsOperationInProgress && CanDefragment && SelectedDrive != null);

        // Initial load - don't call directly to avoid blocking constructor
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            new Action(() => RefreshDrives()),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    private void RefreshDrives()
    {
        IsOperationInProgress = true;
        StatusMessage = "Detectando unidades...";

        try
        {
            try
            {
                AvailableDrives.Clear();
                var drives = _diskService.GetDrives();

                foreach (var drive in drives)
                {
                    AvailableDrives.Add(drive);
                }

                StatusMessage = AvailableDrives.Count > 0
                    ? $"Se detectaron {AvailableDrives.Count} unidades"
                    : "No se detectaron unidades";

                if (AvailableDrives.Count > 0 && SelectedDrive == null)
                {
                    SelectedDrive = AvailableDrives[0];
                }
            }
            catch (Exception innerEx)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshDrives inner error: {innerEx}");
                StatusMessage = $"Error al detectar unidades: {innerEx.Message}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshDrives outer error: {ex}");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsOperationInProgress = false;
        }
    }

    private async void CleanTempFiles()
    {
        if (SelectedDrive == null)
            return;

        await ExecuteOperationAsync(
            () => _diskService.CleanTempFiles(SelectedDrive.DriveLetter), 
            "Limpieza de Archivos Temporales",
            "Se están eliminando archivos temporales de Temp, Prefetch y otras carpetas del sistema.\nEsto puede liberar varios gigabytes de espacio.");
    }

    private async void ExecuteTrim()
    {
        if (SelectedDrive == null)
            return;

        await ExecuteOperationAsync(
            () => _diskService.ExecuteTrim(SelectedDrive.DriveLetter),
            "TRIM de SSD",
            "Se está ejecutando TRIM para optimizar el rendimiento del SSD.\nEsta operación indica al SSD qué bloques están disponibles para su reuso.");
    }

    private async void Defragment()
    {
        if (SelectedDrive == null)
            return;

        await ExecuteOperationAsync(
            () => _diskService.Defragment(SelectedDrive.DriveLetter),
            "Desfragmentación de Disco",
            "Se está desfragmentando el disco duro.\nEsta operación reorganiza los datos para mejorar la velocidad de acceso.\n\n⚠️ NOTA: Esto puede tomar de 15 minutos a varias horas dependiendo del tamaño y fragmentación del disco.");
    }

    private async Task ExecuteOperationAsync(Func<DiskOptimizationReport> operation, string operationName, string description = "")
    {
        if (IsOperationInProgress || SelectedDrive == null)
            return;

        IsOperationInProgress = true;
        StatusMessage = $"⏳ {operationName}...";
        
        // Show initial report with description
        ReportContent = $@"
═══════════════════════════════════════
{operationName.ToUpper()}
═══════════════════════════════════════

Unidad: {SelectedDrive.DriveLetter}
Inicio: {DateTime.Now:HH:mm:ss}

{description}

Estado: Procesando...

═══════════════════════════════════════";
        HasReport = true;

        try
        {
            // Run operation in background thread
            var report = await Task.Run(() => operation());
            
            ReportContent = report.FormattedResult;
            HasReport = true;
            
            StatusMessage = report.Success
                ? $"✓ Operación completada en {SelectedDrive.DriveLetter}"
                : $"✗ Error en operación de {SelectedDrive.DriveLetter}";

            // Refresh drive info to show updated space
            await Task.Run(() => 
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // Don't call RefreshDrives as it might clear selection
                    // Instead, just update the specific drive
                    if (SelectedDrive != null)
                    {
                        var updatedDrive = _diskService.GetDriveInfo(SelectedDrive.DriveLetter);
                        if (updatedDrive != null)
                        {
                            var index = AvailableDrives.IndexOf(SelectedDrive);
                            if (index >= 0)
                            {
                                // Update without triggering SelectedDrive setter
                                AvailableDrives[index] = updatedDrive;
                                _selectedDrive = updatedDrive;
                                OnPropertyChanged(nameof(SelectedDrive));
                            }
                        }
                    }
                });
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ReportContent = $"Error durante la operación:\n\n{ex.Message}\n\n{ex.StackTrace}";
            HasReport = true;
        }
        finally
        {
            IsOperationInProgress = false;
            // Don't update command state here - it's already managed by property setters
        }
    }

    private void UpdateOperationAvailability()
    {
        if (SelectedDrive == null)
        {
            CanExecuteTrim = false;
            CanDefragment = false;
            return;
        }

        try
        {
            var driveType = _diskService.GetDriveType(SelectedDrive.DriveLetter);
            
            // Debug: show what we detected
            System.Diagnostics.Debug.WriteLine($"Drive {SelectedDrive.DriveLetter} detected as: {driveType}");
            
            bool isSSD = driveType.Contains("SSD", StringComparison.OrdinalIgnoreCase);
            bool isHDD = driveType.Contains("HDD", StringComparison.OrdinalIgnoreCase);

            System.Diagnostics.Debug.WriteLine($"isSSD={isSSD}, isHDD={isHDD}");

            CanExecuteTrim = isSSD;
            CanDefragment = isHDD;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UpdateOperationAvailability: {ex.Message}");
            // On error, enable both
            CanExecuteTrim = false;
            CanDefragment = false;
        }
    }
}
