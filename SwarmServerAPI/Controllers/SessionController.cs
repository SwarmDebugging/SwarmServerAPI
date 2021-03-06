﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SwarmServerAPI.Models;

namespace SwarmServerAPI.Controllers
{
    public class SessionController : ApiController
    {
        //OBSERVAÇÃO: Por default, métodos declarados em um Controller como públicos e cujos nomes se iniciem por “Get”, “Post”, “Put” e “Delete” são mapeados automaticamente para o processamento das requisições HTTP correspondentes (GET, POST, PUT e DELETE, respectivamente).
        //https://www.devmedia.com.br/asp-net-web-api-implementando-servicos-restful/31024
        //http://www.ciceroednilson.com.br/criando-um-servico-restful-com-web-api-em-c/
        //Best practices
        //https://blog.mwaysolutions.com/2014/06/05/10-best-practices-for-better-restful-api/

        public IEnumerable<SessionModel> Get()
        {
            try
            {
                using (SwarmData context = new SwarmData())
                {
                    return context.Sessions.Select(s => new SessionModel
                    {
                        Identifier = s.Identifier,
                        Label = s.Label,
                        Description = s.Description,
                        Purpose = s.Purpose,
                        Started = s.Started,
                        Finished = s.Finished ?? DateTime.MinValue,
                        Task = new TaskModel
                        {
                            Action = s.Task.Action,
                            Created = s.Task.Created,
                            Description = s.Task.Description,
                            Name = s.Task.Name,
                            Project = new ProjectModel
                            {
                                Name = s.Task.Project.Name,
                                Description = s.Task.Project.Description
                            }
                        },
                        Developer = new DeveloperModel
                        {
                            Name = s.Developer.Name
                        },
                        Breakpoints = s.Breakpoints.Select(b => new BreakpointModel
                        {
                            BreakpointKind = b.BreakpointKind,
                            Created = b.Created,
                            LineNumber = b.LineNumber ?? 0,
                            LineOfCode = b.LineOfCode,
                            Namespace = b.Namespace,
                            Origin = b.Origin,
                            Type = b.Type
                        }).ToList(),
                        Events = s.Events.Select(e => new EventModel
                        {
                            CharEnd = e.CharEnd ?? 0,
                            CharStart = e.CharStart ?? 0,
                            Created = e.Created,
                            Detail = e.Detail,
                            EventKind = e.EventKind,
                            LineNumber = e.LineNumber ?? 0,
                            LineOfCode = e.LineOfCode,
                            Method = e.Method,
                            MethodKey = e.MethodKey,
                            MethodSignature = e.MethodSignature,
                            Namespace = e.Namespace,
                            Type = e.Type,
                            TypeFullPath = e.TypeFullPath
                        }).ToList(),
                        PathNodes = s.PathNodes.Select(p => new PathNodeModel
                        {
                            Created = p.Created,
                            Type = p.Type,
                            Namespace = p.Namespace,
                            Hash = p.Hash,
                            Method = p.Method,
                            MethodCodeMetric = new CodeMetricModel
                            {
                                ClassCoupling = p.MethodCodeMetric.ClassCoupling,
                                CyclomaticComplexity = p.MethodCodeMetric.CyclomaticComplexity,
                                Hash = p.MethodCodeMetric.Hash,
                                LineOfCode = p.MethodCodeMetric.LineOfCode,
                                MaintainabilityIndex = p.MethodCodeMetric.MaintainabilityIndex
                            },
                            Origin = p.Origin,
                            Parameters = p.Parameters.Select(pp => new PathNodeParameterModel
                            {
                                Name = pp.Name,
                                Type = pp.Type,
                                Value = pp.Value
                            }).ToList(),
                            Parent = p.Parent,
                            ReturnType = p.ReturnType
                        }).ToList()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ThrowError(ex);
            }
        }

        public string Post(Session session)
        {
            try
            {
                using (SwarmData context = new SwarmData())
                {
                    Session original = context.Sessions.FirstOrDefault(s => s.Identifier == session.Identifier);

                    if (original == null)
                        context.Sessions.Add(session);
                    else
                        context.Entry(original).CurrentValues.SetValues(session);

                    context.SaveChanges();

                    return "Object created or updated!";
                }
            }
            catch (Exception ex)
            {
                throw ThrowError(ex);
            }
        }

        private HttpResponseException ThrowError(Exception ex)
        {
            //TODO: bad smell return internal error. Review later.
            var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.ToString()),
                ReasonPhrase = "Error!"
            };

            return new HttpResponseException(resp);
        }
    }
}
