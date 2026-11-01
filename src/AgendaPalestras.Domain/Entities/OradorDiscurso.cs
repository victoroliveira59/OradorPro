using AgendaPalestras.Domain.Common;

namespace AgendaPalestras.Domain.Entities;

public class OradorDiscurso : BaseEntity
{
    public Guid OradorId { get; private set; }
    public Orador Orador { get; private set; } = null!;

    public Guid DiscursoId { get; private set; }
    public Discurso Discurso { get; private set; } = null!;

    protected OradorDiscurso() { }

    public OradorDiscurso(Guid oradorId, Guid discursoId)
    {
        OradorId = oradorId;
        DiscursoId = discursoId;
    }
}
