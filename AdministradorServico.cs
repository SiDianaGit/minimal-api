using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;

namespace MinimalApi.Dominio.Servicos
{
    public class AdministradorServico : iAdministradorServico
    {
        private readonly DBContexto _contexto;
        public AdministradorServico(DBContexto contexto)
        {
            _contexto = contexto;
        }

        public DBContexto Contexto => _contexto;

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Adminitradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
            
            
            //throw new NotImplementedException();
        }

        public void Incluir(Administrador administrador)
        {
             _contexto.Adminitradores.Add(administrador);
            _contexto.SaveChanges();
                   
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
           var query = _contexto.Adminitradores.AsQueryable();
           
           int itensPorPagina = 10;

           if(pagina != null) 
           {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
           }

           return query.ToList();
        } 

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Adminitradores.Where(v => v.id == id).FirstOrDefault();
        }    

    }
}