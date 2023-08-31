using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodexAPI.Models.Enums;

namespace CodexAPI.Models
{
    public class Requisicao : BaseModel
    {
        

        public StatusRequisicao Status { get; set; }

        public string Notas { get; set; } = "";

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }
    }
}