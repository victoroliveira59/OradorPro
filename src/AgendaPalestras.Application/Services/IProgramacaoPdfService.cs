using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Application.Services;

public interface IProgramacaoPdfService
{
    byte[] GerarPdfProgramacaoFimDeSemana(
        string nomeCongregacao,
        int mes,
        int ano,
        IReadOnlyList<AgendaDto> agendamentos);
}
