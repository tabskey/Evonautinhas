using System;

namespace Evonautinhas.Domain.Exceptions
{
    /// <summary>
    /// Sinaliza que já existe um aluno arquivado (Ativo = false) com o e-mail informado.
    /// Permite que a API ofereça a reativação em vez de recusar um novo cadastro duplicado.
    /// </summary>
    public class ArchivedStudentException : Exception
    {
        public int AlunoId { get; }

        public ArchivedStudentException(int alunoId, string message)
            : base(message)
        {
            AlunoId = alunoId;
        }
    }
}
