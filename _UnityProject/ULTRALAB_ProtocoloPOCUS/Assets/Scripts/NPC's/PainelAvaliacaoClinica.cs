
using TMPro;
using UnityEngine;

public sealed class PainelAvaliacaoClinica : MonoBehaviour
{
    [System.Serializable]
    public class BotaoCategoria
    {
        public GameObject botao;
        public Categoria categoria;
    }

    public enum Categoria
    {
        ViaAerea,
        Respiracao,
        Consciencia,
        Pele,
        Nutricao
    }

    [Header("Painéis")]
    [SerializeField] private GameObject painelAvaliacao;
    [SerializeField] private GameObject painelDialogo;

    [Header("Diálogo")]
    [SerializeField] private TMP_Text tituloDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [Header("Categorias")]
    [SerializeField] private BotaoCategoria[] botoesCategorias;

    private void Start()
    {
        if (painelAvaliacao != null)
            painelAvaliacao.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    public void AbrirAvaliacao()
    {
        if (painelAvaliacao != null)
            painelAvaliacao.SetActive(true);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    public void FecharAvaliacao()
    {
        if (painelAvaliacao != null)
            painelAvaliacao.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    public void FecharDialogo()
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    public void MostrarViaAerea()
    {
        MostrarCategoria(Categoria.ViaAerea);
    }

    public void MostrarRespiracao()
    {
        MostrarCategoria(Categoria.Respiracao);
    }

    public void MostrarConsciencia()
    {
        MostrarCategoria(Categoria.Consciencia);
    }

    public void MostrarPele()
    {
        MostrarCategoria(Categoria.Pele);
    }

    public void MostrarNutricao()
    {
        MostrarCategoria(Categoria.Nutricao);
    }

    private void MostrarCategoria(Categoria categoria)
    {
        PatientData paciente = CurrentPatient.Data;

        if (paciente == null)
        {
            MostrarDialogo("Paciente", "Nenhum paciente foi selecionado.");
            return;
        }

        int dia = Timer.Instance != null
            ? Mathf.Max(1, Timer.Instance.CurrentDay)
            : 1;

        PatientData dados = PatientDailyState.GetClinicalData(
            paciente, dia);

        if (dados == null)
        {
            MostrarDialogo(
                "Avaliação clínica",
                "Não foi possível consultar os dados deste paciente.");
            return;
        }

        string titulo;
        string pista;

        switch (categoria)
        {
            case Categoria.ViaAerea:
                titulo = "Via Aérea";
                pista = CriarPistaViaAerea(dados);
                break;

            case Categoria.Respiracao:
                titulo = "Respiração";
                pista = CriarPistaRespiracao(dados);
                break;

            case Categoria.Consciencia:
                titulo = "Consciência";
                pista = CriarPistaConsciencia(dados);
                break;

            case Categoria.Pele:
                titulo = "Pele e Exposição";
                pista = CriarPistaPele(dados);
                break;

            case Categoria.Nutricao:
                titulo = "Nutrição e Suporte";
                pista = CriarPistaNutricao(dados);
                break;

            default:
                titulo = "Avaliação clínica";
                pista = "Não há informações disponíveis.";
                break;
        }

        MostrarDialogo(titulo, pista);
    }

    private string CriarPistaViaAerea(PatientData dados)
    {
        string permeabilidade = Normalizar(dados.permeabilidade);
        string presenca = Normalizar(dados.presenca);
        string intervencao = Normalizar(dados.intervencao);

        string resultado = "";

        if (Contem(permeabilidade, "pérvia", "pervia"))
        {
            resultado =
                "O ar parece passar sem dificuldade. Não há sinais aparentes de obstrução.";
        }
        else if (Contem(permeabilidade, "parcial"))
        {
            resultado =
                "A passagem de ar parece estar parcialmente comprometida. Observe atentamente a via aérea.";
        }
        else if (Contem(permeabilidade, "obstru"))
        {
            resultado =
                "Há indícios de dificuldade na passagem de ar. A via aérea exige atenção imediata.";
        }

        if (Contem(presenca, "secre"))
        {
            resultado +=
                "\nHá indícios de secreções que podem interferir na passagem de ar.";
        }

        if (Contem(presenca, "corpo estranho"))
        {
            resultado +=
                "\nExiste suspeita de material estranho na via aérea.";
        }

        if (Contem(intervencao, "sim", "necess"))
        {
            resultado +=
                "\nOs dados sugerem que pode ser necessária uma intervenção na via aérea.";
        }

        if (string.IsNullOrWhiteSpace(resultado))
        {
            resultado =
                "Observe a passagem de ar e procure sinais de obstrução ou conteúdo na via aérea.";
        }

        return resultado;
    }

    private string CriarPistaRespiracao(PatientData dados)
    {
        string padrao = Normalizar(dados.padraoRespiratorio);
        string acessoria = Normalizar(dados.acessoria);
        string ausculta = Normalizar(dados.asculta);

        string resultado = "";

        if (Contem(padrao, "eupne"))
        {
            resultado =
                "A respiração parece ocorrer em ritmo regular, sem alterações evidentes.";
        }
        else if (Contem(padrao, "taquipne"))
        {
            resultado =
                "O paciente parece respirar mais rapidamente do que o esperado.";
        }
        else if (Contem(padrao, "bradipne"))
        {
            resultado =
                "A respiração parece mais lenta do que o esperado.";
        }
        else if (Contem(padrao, "irregular"))
        {
            resultado =
                "O ritmo respiratório apresenta irregularidades.";
        }

        if (Contem(acessoria, "sim", "presente", "uso"))
        {
            resultado +=
                "\nÉ possível observar esforço adicional durante a respiração.";
        }
        else if (Contem(acessoria, "não", "nao", "ausente"))
        {
            resultado +=
                "\nNão há uso evidente de musculatura acessória.";
        }

        if (Contem(ausculta, "murmúrio vesicular presente",
                "murmurio vesicular presente"))
        {
            resultado +=
                "\nOs sons respiratórios habituais estão presentes.";
        }
        else if (Contem(ausculta, "reduzido"))
        {
            resultado +=
                "\nOs sons respiratórios parecem estar diminuídos em alguma região.";
        }
        else if (Contem(ausculta, "estertores"))
        {
            resultado +=
                "\nÉ possível identificar sons respiratórios adicionais, semelhantes a estalidos.";
        }
        else if (Contem(ausculta, "sibilos"))
        {
            resultado +=
                "\nHá sons respiratórios semelhantes a chiados.";
        }

        if (string.IsNullOrWhiteSpace(resultado))
        {
            resultado =
                "Observe o ritmo, o esforço respiratório e os sons produzidos durante a respiração.";
        }

        return resultado;
    }

    private string CriarPistaConsciencia(PatientData dados)
    {
        string consciencia = Normalizar(dados.nivelConsciencia);

        if (Contem(consciencia, "alerta", "consciente"))
        {
            return "O paciente mantém os olhos abertos e parece atento ao ambiente ao redor.";
        }

        if (Contem(consciencia, "responde à voz",
                "responde a voz", "voz"))
        {
            return "O paciente parece reagir quando alguém fala com ele, mas não mantém o mesmo nível de interação espontânea.";
        }

        if (Contem(consciencia, "responde à dor",
                "responde a dor"))
        {
            return "O paciente não apresenta resposta verbal clara, mas demonstra reação quando submetido a estímulos dolorosos por um profissional.";
        }

        if (Contem(consciencia, "inconsciente"))
        {
            return "O paciente não demonstra resposta perceptível à voz nem aos estímulos habituais. É necessária avaliação imediata.";
        }

        return "Observe se o paciente mantém contato, reage à voz ou responde a estímulos.";
    }

    private string CriarPistaPele(PatientData dados)
    {
        string pele = Normalizar(dados.avalicaoPele);
        string alteracoes = Normalizar(dados.presencaPele);

        string resultado = "";

        if (Contem(pele, "íntegra", "integra"))
        {
            resultado =
                "A superfície da pele parece preservada, sem lesões aparentes.";
        }

        if (Contem(pele, "lesões", "lesoes"))
        {
            resultado +=
                "\nHá alterações visíveis na integridade da pele.";
        }

        if (Contem(pele, "cianose"))
        {
            resultado +=
                "\nObserve uma possível coloração azulada da pele.";
        }

        if (Contem(pele, "palidez"))
        {
            resultado +=
                "\nA coloração da pele parece mais pálida do que o habitual.";
        }

        if (Contem(pele, "sudorese"))
        {
            resultado +=
                "\nA pele apresenta sinais visíveis de sudorese.";
        }

        if (Contem(alteracoes, "hemorragia"))
        {
            resultado +=
                "\nHá sinais visíveis de sangramento que precisam ser avaliados.";
        }

        if (Contem(alteracoes, "lesões por pressão",
                "lesoes por pressao"))
        {
            resultado +=
                "\nExistem alterações cutâneas em regiões de pressão.";
        }

        if (string.IsNullOrWhiteSpace(resultado))
        {
            resultado =
                "Observe a coloração, a integridade da pele e a presença de alterações visíveis.";
        }

        return resultado;
    }

    private string CriarPistaNutricao(PatientData dados)
    {
        string resumo = dados.resumeCaso;

        if (string.IsNullOrWhiteSpace(resumo))
        {
            return "Consulte o histórico clínico e verifique como o paciente está recebendo suporte nutricional.";
        }

        return "O histórico clínico contém informações relevantes sobre a condição e a evolução do paciente.\n\n" +
               "Revise o prontuário para investigar possíveis dificuldades nutricionais e a necessidade de suporte.";
    }

    private void MostrarDialogo(string titulo, string mensagem)
    {
        if (tituloDialogo != null)
            tituloDialogo.text = titulo;

        if (textoDialogo != null)
            textoDialogo.text = mensagem;

        if (painelDialogo != null)
            painelDialogo.SetActive(true);
    }

    private static string Normalizar(string valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? ""
            : valor.Trim().ToLowerInvariant();
    }

    private static bool Contem(string valor, params string[] termos)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        foreach (string termo in termos)
        {
            if (valor.Contains(termo))
                return true;
        }

        return false;
    }
}
