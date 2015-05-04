﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEGF.Dominio;
using AEGF.Dominio.Servicos;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AEGF.BancosViaSite
{
    public class SantanderSite: AcessoSelenium, IBancoAcesso
    {
        private Banco _banco;
        private ICollection<Extrato> _extratos;
        private int _cartaoAtual;

        public IEnumerable<Extrato> LerExtratos()
        {
            IniciarBrowser();
            Inicio();
            FazerLogin();
            LerExtrato();
            LerCartoes();
            return _extratos;
        }

        private void LerCartoes()
        {
            do
            {
                _cartaoAtual++;
                ClicaFatura();
            } while (LerCartaoAtual());
        }

        private bool LerCartaoAtual()
        {
            VaiParaIFramePrinc();
            var trs = driver.FindElements(By.CssSelector("table.lista tr.trClaro"));
            if (trs.Count > _cartaoAtual)
                return false;

            var linha = trs[_cartaoAtual];
            var link = linha.FindElement(By.XPath("//*td[2]/a"));
            link.Click();

            VaiParaIFramePrinc();
            // todo guardar parte da referencia aqui
            /*
             * 	detalhes = iframePrinc.find("div.caixa td.bold");
	if (detalhes.length <= 0 )
		return;
		
	retorno.bandeira = detalhes[0].innerText;
	retorno.conta = detalhes[2].innerText;

             */

            TrocaFrameId("iDetalhes");


            var extrato = CriaRetorno("#detfatura tr.trClaro", true, 0, 1, 2);
            extrato.Referencia = "30Dias";
            extrato.Descricao = "Conta Corrente";
            _extratos.Add(extrato);

            return true;
        }

        private void ClicaFatura()
        {
            VaiParaMenu();
            ClicaXPath("//*[@id=\"3975Link\"]");
            VaiParaCorpo();
            ClicaXPath("//*[@id=\"montaMenu\"]/ul/li[1]/ul/li[2]/a");
        }


        private void VaiParaMenu()
        {
            driver.SwitchTo().DefaultContent();
            VaiParaFramePrincipal();
            TrocaFrameXPath("//*[@id=\"frameMain\"]/frame[1]");
        }

        private void LerExtrato()
        {
            SelecionaPeriodo();
            LerTabelaExtrato();
        }

        private void LerTabelaExtrato()
        {
            VaiParaIFramePrinc();
            TrocaFrameXPath("//*[@id=\"extrato\"]");
            var extrato = CriaRetorno("table.lista tr.trClaro", false, 0, 2, 5);
            extrato.Referencia = "30Dias";
            extrato.Descricao = "Conta Corrente";
            _extratos.Add(extrato);

        }

        private Extrato CriaRetorno(string cssPath, bool tipoCartao, int colData, int colDescricao, int colValor)
        {
            var trs = driver.FindElements(By.CssSelector(cssPath));
            var retorno = new Extrato { CartaoCredito = tipoCartao };

            AdicionaItens(retorno, trs, colData, colDescricao, colValor);

            if (retorno.CartaoCredito)
            {
                retorno.Referencia = driver.FindElement(By.XPath("table.transacao strong")).Text;
            }

            return retorno;
        }

        private static void AdicionaItens(Extrato extrato, ReadOnlyCollection<IWebElement> linhas, int colData, int colDescricao, int colValor)
        {
            foreach (var linha in linhas)
            {
                var colunas = linha.FindElements(By.TagName("td"));

                var valorStr = colunas[colValor].Text;
                double valor;

                if (Double.TryParse(valorStr, out valor))
                {
                    var item = new Transacao()
                    {
                        Valor = valor,
                        Descricao = colunas[colDescricao].Text,
                        Data = DateTime.Parse(colunas[colData].Text)
                    };
                    extrato.AdicionaTransacao(item);

                }
            }
        }

        private void VaiParaIFramePrinc()
        {
            VaiParaCorpo();
            TrocaFrameXPath("//*[@id=\"iframePrinc\"]");
        }

        private void SelecionaPeriodo()
        {
            VaiParaCorpo();
            TrocaFrameXPath("//*[@id=\"iframePainel\"]");
            SelecionaValorXPath("//*[@id=\"select\"]/select", "30");
            ClicaXPath("//*[@id=\"extrato\"]/tbody/tr/td[3]/a");
        }

        private void FazerLogin()
        {
            TrocaFrameId("iframetopo");

            DigitaTextoId("txtCPF", _banco.LerConfiguracao("CPF") /*_banco.Configuracoes.Single(configuracao => configuracao.Nome == "CPF").Valor*/);
            ClicaId("hrefOk");
            FechaMensagemPlugin();

            DigitaTextoId("txtSenha", _banco.LerConfiguracao("Senha")/*_banco.Configuracoes.Single(configuracao => configuracao.Nome == "Senha").Valor*/);
            ClicaXPath("//*[@id=\"divBotoes\"]/a[1]");
        }

        private void FechaMensagemPlugin()
        {
            VaiParaFramePrincipal();

            TrocaFrameNome("MainFrame");
            ClicaXPath("//*[@id=\"divFloaterStormFish\"]/div/map/area[1]");
        }


        private void VaiParaFramePrincipal()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(webDriver =>
            {
                try
                {
                    return webDriver.FindElement(By.Id("frmSet")) != null;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            TrocaFrameXPath("//*[@id=\"frmSet\"]/frame[2]");
        }

        private void VaiParaCorpo()
        {
            VaiParaFramePrincipal();
            TrocaFrameXPath("//*[@id=\"frameMain\"]/frame[2]");
            
        }

        private void Inicio()
        {
            _extratos = new List<Extrato>();
            _cartaoAtual = 0;
        }

        public string NomeUnico()
        {
            return "SantanderSite";
        }

        public void Iniciar(Banco banco)
        {
            _banco = banco;
        }

        protected override string URLSite()
        {
            return "http://www.santander.com.br";
        }
    }
}
