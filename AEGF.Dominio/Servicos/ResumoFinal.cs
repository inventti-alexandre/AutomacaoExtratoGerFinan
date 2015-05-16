﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AEGF.Dominio.Repositorios;

namespace AEGF.Dominio.Servicos
{
    public class ResumoFinal: IResumoFinal
    {
        private readonly IExtratoRepositorio _repositorio;
        private readonly IFormatadorResumo _formatador;

        public ResumoFinal(IExtratoRepositorio repositorio, IFormatadorResumo formatador)
        {
            _repositorio = repositorio;
            _formatador = formatador;
        }

        public void CriarResumo(string arquivoSaida, IEnumerable<Extrato> extratos)
        {
            VerificarNovosRegistros(extratos);
            _formatador.Formatar(arquivoSaida, extratos);
        }

        private void VerificarNovosRegistros(IEnumerable<Extrato> extratos)
        {
            foreach (var extrato in extratos)
            {
                var extratosSalvos = _repositorio.ObterPorDescricaoReferencia(extrato.Descricao, extrato.Referencia);
                var ultimo = extratosSalvos.LastOrDefault() ?? new Extrato();
                MarcarVelhas(ultimo, extrato);
                extrato.CalcularTotais();
                _repositorio.Adicionar(extrato);
                DeixarApenasUltimos(extratosSalvos);
            }
        }

        private void DeixarApenasUltimos(IEnumerable<Extrato> extratosSalvos)
        {
            var total = extratosSalvos.Count();
            if (total > 4)
            {
                total = total - 4;
                var paraExcluir = extratosSalvos.Take(total);
                foreach (var extrato in paraExcluir)
                {
                    _repositorio.Excluir(extrato);
                }
            }
        }

        private void MarcarVelhas(Extrato ultimo, Extrato atual)
        {
            foreach (var transacaoVelha in ultimo.Transacoes)
            {
                var transacaoNova = atual.Transacoes.SingleOrDefault(transacao => transacao.Equals(transacaoVelha));
                if (transacaoNova == null)
                    continue;
                transacaoNova.Nova = false;
            }
        }
    }
}
