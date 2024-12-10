using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Infrastructures.Repository.IRepository
{
    public interface IContactUsRepository : IRepository<ContactUs>
    {
    }
}
