using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodexAPI.Models
{
    public class Requisicao
    {
        public int RequisicaoId { get; set; }

        public string Status { get; set; }

        public string Notas { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }
    }
}