using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationalIdentity.EntityFrameworkExtensions
{
    public static class DbExtensions
    {
        /*
         * Credit to Graham O'Neale
         * http://stackoverflow.com/questions/3642371/how-to-update-only-one-field-using-entity-framework
         */
        public static bool Update<T>(this DbContext context, T entity, params string[] properties) where T : class, new()
        {
            if (!context.Set<T>().Local.Contains(entity))
                context.Set<T>().Attach(entity);
            foreach (var property in properties)
            {
                context.Entry(entity).Property(property).IsModified = true;
            }
            return true;
        }

        /*
         * Credit to Ladislav Mrnka
         * http://stackoverflow.com/questions/5749110/readonly-properties-in-ef-4-1/5749469#5749469
         */
        public static bool Update<T>(this DbContext context, T entity, params Expression<Func<T, object>>[] properties) where T : class, new()
        {
            if (!context.Set<T>().Local.Contains(entity))
                context.Set<T>().Attach(entity);
            foreach (var selector in properties)
            {
                context.Entry(entity).Property(selector).IsModified = true;
            }
            return true;
        }
    }
}
