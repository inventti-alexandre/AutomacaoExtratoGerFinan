﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEGF.BancosViaSite;
using AEGF.Dominio;
using AEGF.Dominio.Servicos;
using AEGF.GFViaSite;
using AEGF.RepositorioJson;
using AEGF.ServicoAplicacao;

namespace AutomacaoExtratoGerFinanConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var repositorio = new BancoRepositorio();
            var repositorioGF = new GerenciadorFinanceiroRepositorio();

            var gerenciador = repositorioGF.ObterTodos().First();
            var gerenciadorFinanceiro = new MinhasEconomiasViaSite();
            //var gerenciadorFinanceiro = new GerenciadorFinanceiroAcessoConsole();


            var gerenciadorBanco = new GerenciadorBancoAcesso();
            gerenciadorBanco.AdicionaBancoAcesso(new SantanderSite());

            var integrador = new IntegrarServicoAplicacao(repositorio, gerenciadorFinanceiro, gerenciadorBanco);
            integrador.IntegrarContas();
        }
    }
}
