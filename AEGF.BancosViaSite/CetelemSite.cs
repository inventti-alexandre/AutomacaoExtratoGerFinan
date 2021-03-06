﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AEGF.Dominio;
using AEGF.Dominio.Servicos;
using AEGF.Infra;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace AEGF.BancosViaSite
{
    public class CetelemSite: AcessoSelenium, IBancoAcesso
    {
        private Banco _banco;
        private ICollection<Extrato> _extratos;
        private int _cartaoAtual;

        public IEnumerable<Extrato> LerExtratos()
        {
            IniciarBrowser();
            Inicio();
            FazerLogin();
            LerCartoes();
            FecharBrowser();
            return _extratos;
        }

        private void LerCartoes()
        {
            ClicaXPath("//a[text()='Consulte sua fatura']", true);
            do
            {
                _cartaoAtual += 2;
            } while (LerCartaoAtual());
        }

        private bool LerCartaoAtual()
        {
            // lista dos cartões disponiveis não selecionados //div[@data-tst='card.list.item.name']

            var elementos = driver.FindElements(By.XPath("//td[contains(@onclick, 'changeCard')]"));
            if (_cartaoAtual >= elementos.Count)
                return false;
            Actions builder = new Actions(driver);
            var elemento = elementos[_cartaoAtual];
            builder.MoveToElement(elemento).Click().Perform();
            LerMeses();


            return true;
        }

        private void LerMeses()
        {
            for (int i = 0; i < 3; i++)
            {
                var extrato = new Extrato()
                {
                    CartaoCredito = true,
                };

                var elementos = driver.FindElements(By.XPath("//a[contains(@onclick, '_fatura')]"));
                if (i >= elementos.Count)
                    return;
                Actions builder = new Actions(driver);
                var elemento = elementos[i];
                var texto = elemento.Text;
                builder.MoveToElement(elemento).Click().Perform();

                LerInfo(extrato, texto, i);
                LerMovimentacoes(extrato);

                if (extrato.Transacoes.Any())
                    _extratos.Add(extrato);
            }
        }

        private void LerMovimentacoes(Extrato extrato)
        {
            var linhas = driver.FindElements(By.XPath("//*[@id=\"boxFatura\"]/table/tbody/tr[1]/td/table/tbody/tr"));
            for (int i = 1; i < linhas.Count; i++)
            {
                var linha = linhas[i];
                var tds = linha.FindElements(By.TagName("td"));
                if (tds.Count != 4)
                    continue;

                var transacao = new Transacao
                {
                    Descricao = tds[1].Text.Trim(),
                    Data = DateTime.Parse(tds[0].Text.Trim())
                };
                var sinal = 1;
                var valorStr = "";
                if (string.IsNullOrWhiteSpace(tds[3].Text))
                {
                    sinal = -1;
                    valorStr = tds[2].Text;
                }
                else
                {
                    valorStr = tds[3].Text;
                }

                valorStr = valorStr.Trim();

                transacao.Valor = double.Parse(valorStr);
                transacao.Valor = transacao.Valor*sinal;

                extrato.AdicionaTransacao(transacao);

            }
        }

        private void LerInfo(Extrato extrato, string mes, int indice)
        {
            extrato.Descricao = LerTextoXPath("//*[@id=\"informacoesCartaoPagueFatura\"]/span[1]");
            extrato.Referencia = CriaDataReferencia(mes, indice);
        }

        private DateTime CriaDataReferencia(string mes, int indice)
        {
            int iMes = 1;
            switch (mes)
            {
                case "Janeiro":
                    iMes = 1;
                    break;
                case "Fevereiro":
                    iMes = 2;
                    break;
                case "Março":
                    iMes = 3;
                    break;
                case "Abril":
                    iMes = 4;
                    break;
                case "Maio":
                    iMes = 5;
                    break;
                case "Junho":
                    iMes = 6;
                    break;
                case "Julho":
                    iMes = 7;
                    break;
                case "Agosto":
                    iMes = 8;
                    break;
                case "Setembro":
                    iMes = 9;
                    break;
                case "Outubro":
                    iMes = 10;
                    break;
                case "Novembro":
                    iMes = 11;
                    break;
                case "Dezembro":
                    iMes = 12;
                    break;
            }
            var data = new DateTime(DateTime.Today.Year, iMes, 1);

            if ((indice == 0) && (data > DateTime.Today))
                data = data.AddYears(-1);

            return data;
        }


        private void FazerLogin()
        {
            var id = "_fastAccess_INSTANCE_fastAccess_login";
            AguardarId(id, false);
            
            var query = driver.FindElements(By.Id(id));
            IWebElement elementoT = null;
            foreach (var element in query)
            {
                if (element.Enabled)
                    elementoT = element;
            }
            elementoT.SendKeys(_banco.LerConfiguracao("usuario"));


            query = driver.FindElements(By.Id(id));
            elementoT = null;
            foreach (var element in query)
            {
                if (element.Enabled)
                    elementoT = element;
            }
            elementoT.Click();

            AguardarXPath("//input[@name='_login_password']");
            var senha = _banco.LerConfiguracao("senha");
            foreach (var letra in senha)
            {
                var elemento =
                    driver.FindElement(
                        By.XPath($"//button[@data-tst='{letra.ToString()}']"));
                Actions builder = new Actions(driver);
                builder.MoveToElement(elemento).Click().Perform();
            }
            ClicaId("_login_rdaa");
        }


        private void Inicio()
        {
            _extratos = new List<Extrato>();
            _cartaoAtual = -2;
        }

        public string NomeUnico()
        {
            return "CetelemSite";
        }

        public void Iniciar(Banco banco)
        {
            _banco = banco;
        }

        protected override string URLSite()
        {
            return "http://www.cetelem.com.br";
        }

    }
}
