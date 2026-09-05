namespace Evonautinhas.Domain.Entities
{
    public class RelatorioAlunosPorTurma
    {
        public int TurmaId { get; set; }
        public string TurmaNome { get; set; }
        public string Periodo { get; set; }
        public int VagasTotal { get; set; }
        public int VagasDisponiveis { get; set; }
        public int TotalAlunos { get; set; }
    }
}