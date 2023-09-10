namespace CodexAPI.Models.Enums
{
    public enum StatusRequisicao
    {
        Pendente = 1,
        TextoBaseInvalido = 2,

        AguardandoProcessamento = 3,
        
    
        FalhaProcessamento = 4,

        Processando = 5,

        AguardandoPerguntasRespostas = 6,

        FalhaPerguntasRespostas = 7,

        PerguntasRespostasObtidas =  8,

        AguardandoValidacao = 9,

        FalhaValidacao = 10,

        Pronto = 11,
        FalhaGenerica = 12
        
    }
}