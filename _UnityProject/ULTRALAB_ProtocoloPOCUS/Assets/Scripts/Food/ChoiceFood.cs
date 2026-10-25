using UnityEngine;
using UnityEngine.UI;

public class ChoiceFood : MonoBehaviour, IInteractable
{
    public enum TipoComida
    {
        Nenhuma,
        Solido,
        Liquido,
        NPT
    }

    [System.Serializable]
    public class OpcaoComida
    {
        public TipoComida tipo;
        public Button botao;
        public Sprite sprite;

        [HideInInspector]
        public Color corOriginal;
    }

    [Header("Painel de Escolha")]
    [SerializeField] private GameObject painelComida;
    [SerializeField] private OpcaoComida[] opcoes;

    [Header("Comida Selecionada na Tela Principal")]
    [SerializeField] private Image imagemComidaSelecionada;

    [Header("Aparência")]
    [SerializeField] private Color corSelecionada = Color.gray;

    public TipoComida ComidaSelecionada { get; private set; }
        = TipoComida.Nenhuma;

    public Sprite SpriteSelecionado { get; private set; }

    private TipoComida selecaoProvisoria = TipoComida.Nenhuma;
    private bool painelAberto;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        AbrirPainel();
    }
    
    private void Awake()
    {
        if (painelComida != null)
            painelComida.SetActive(false);

        // Guarda as cores originais dos botões.
        foreach (OpcaoComida opcao in opcoes)
        {
            if (opcao == null || opcao.botao == null)
                continue;

            Image imagem = opcao.botao.targetGraphic as Image;

            if (imagem != null)
                opcao.corOriginal = imagem.color;
        }

        // Mantém a interface sincronizada com a escolha atual.
        AtualizarImagemPrincipal();
    }

    public void AbrirPainel()
    {
        if (painelComida == null)
            return;

        painelAberto = true;
        selecaoProvisoria = TipoComida.Nenhuma;

        RestaurarCores();

        painelComida.SetActive(true);
    }

    public void SelecionarComida(int indice)
    {
        if (!painelAberto || indice < 0 || indice >= opcoes.Length)
            return;

        OpcaoComida opcao = opcoes[indice];

        if (opcao == null || opcao.tipo == TipoComida.Nenhuma)
            return;

        // Primeiro clique: seleciona provisoriamente.
        if (selecaoProvisoria != opcao.tipo)
        {
            selecaoProvisoria = opcao.tipo;

            RestaurarCores();
            AlterarCor(opcao, corSelecionada);

            return;
        }

        // Segundo clique na mesma opção: confirma.
        ComidaSelecionada = opcao.tipo;
        SpriteSelecionado = opcao.sprite;

        AtualizarImagemPrincipal();

        FecharPainel();
    }

    public void FecharPainel()
    {
        painelAberto = false;
        selecaoProvisoria = TipoComida.Nenhuma;

        RestaurarCores();

        if (painelComida != null)
            painelComida.SetActive(false);
    }

    private void AlterarCor(OpcaoComida opcao, Color cor)
    {
        if (opcao.botao == null)
            return;

        Image imagem = opcao.botao.targetGraphic as Image;

        if (imagem != null)
            imagem.color = cor;
    }

    private void RestaurarCores()
    {
        foreach (OpcaoComida opcao in opcoes)
        {
            if (opcao == null || opcao.botao == null)
                continue;

            Image imagem = opcao.botao.targetGraphic as Image;

            if (imagem != null)
                imagem.color = opcao.corOriginal;
        }
    }


    private void AtualizarImagemPrincipal()
    {
        if (imagemComidaSelecionada == null)
            return;

        bool temComidaSelecionada = SpriteSelecionado != null;

        imagemComidaSelecionada.sprite = SpriteSelecionado;
        imagemComidaSelecionada.enabled = temComidaSelecionada;
    }



    public void Voltar()
    {
        // Fecha o painel sem confirmar uma nova comida.
        FecharPainel();
    }

}
