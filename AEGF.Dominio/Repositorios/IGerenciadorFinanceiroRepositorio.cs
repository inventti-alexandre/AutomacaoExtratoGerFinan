﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AEGF.Dominio.Repositorios
{
    public interface IGerenciadorFinanceiroRepositorio
    {
        IEnumerable<GerenciadorFinanceiro> ObterTodos();

    }
}