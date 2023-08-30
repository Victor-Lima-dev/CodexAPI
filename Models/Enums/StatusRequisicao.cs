namespace CodexAPI.Models.Enums
{
    public enum StatusRequisicao
    {
        Pendente = 1,
        TextoBaseInvalido = 2,

        AguardandoProcessamento = 3,

        Processando = 4,

        AguardandoPerguntasRespostas = 5,

        FalhaPerguntasRespostas = 6,

        PerguntasRespostasObtidas = 7,

        AguardandoValidacao = 8,

        FalhaValidacao = 9,

        Pronto = 10,
        FalhaGenerica = 11
        
    }
}