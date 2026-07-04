using System.Collections.ObjectModel;
using SistemaRH.Application.DTOs;

namespace SistemaRH.Desktop.ViewModels;

public class PeriodoFeriasComUsos
{
    public PeriodoAquisitivoDto Periodo { get; set; } = null!;
    public ObservableCollection<PeriodoFeriasDto> Usos { get; set; } = new();
    public bool TemUsos => Usos.Count > 0;
}
