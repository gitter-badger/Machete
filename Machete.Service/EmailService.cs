﻿using Machete.Data;
using Machete.Data.Infrastructure;
using Machete.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Machete.Service
{
    public interface IEmailService : IService<Email>
    {
        dataTableResult<Email> GetIndexView(viewOptions o);
        Email GetLatestConfirmEmailBy(int woid);
        IEnumerable<Email> GetMany(Func<Email, bool> predicate);
        IEnumerable<Email> GetEmailsToSend();
        Email GetExclusive(int eid, string user);
    }

    public class EmailService : ServiceBase<Email>, IEmailService
    {
        IWorkOrderService _woServ;

        public EmailService(IEmailRepository emRepo, IWorkOrderService woServ, IUnitOfWork uow) : base(emRepo, uow)
        {
            this.logPrefix = "Email";
            _woServ = woServ;
        }

        public Email GetLatestConfirmEmailBy(int woid)
        {
            var wo = _woServ.Get(woid);
            if (wo == null) throw new MacheteServiceException("Cannot find workorder.");
            var emailJoiner = wo.JoinWorkorderEmails.OrderByDescending(e => e.datecreated).FirstOrDefault();
            if (emailJoiner == null) {return null;}
            return emailJoiner.Email;
        }

        public Email GetExclusive(int eid, string user)
        {
            var e =  repo.GetById(eid);
            if (e.status == Email.iSending ||
                e.status == Email.iSent)
            {
                return null;
            }
            // user will have to re-send when editing a ReadyToSend
            if (e.status == Email.iReadyToSend)
            {
                e.status = Email.iPending;
            }
            // transmit errors will remain as error re-sent
            e.updatedby(user);
            log(e.ID, user, logPrefix + " get exclusive for email");
            uow.Commit();
            return e;
        }

        public IEnumerable<Email> GetMany(Func<Email, bool> predicate)
        {
            return repo.GetMany(predicate);
        }

        public IEnumerable<Email> GetEmailsToSend()
        {
            return repo.GetManyQ()
                       .Where(e => e.status == Email.iReadyToSend ||
                           (e.status == Email.iTransmitError && e.transmitAttempts < 10)
                           )
                       .ToList();
        }

        public dataTableResult<Email> GetIndexView(viewOptions o)
        {
            var result = new dataTableResult<Email>();
            IQueryable<Email> q = repo.GetAllQ();
            result.filteredCount = q.Count();
            result.totalCount = repo.GetAllQ().Count();
            result.query = q.OrderBy(e => e.ID).Skip(o.displayStart).Take(o.displayLength);
            return result;
        }
    }
}
