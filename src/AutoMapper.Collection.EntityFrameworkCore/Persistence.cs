using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AutoMapper.EntityFrameworkCore
{
    public class Persistence<TTo> : IPersistence<TTo>
        where TTo : class
    {
        private readonly DbSet<TTo> _sourceSet;
        private readonly IMapper _mapper;

        public Persistence(DbSet<TTo> sourceSet, IMapper mapper)
        {
            _sourceSet = sourceSet;
            _mapper = mapper;
        }

        public TTo InsertOrUpdate<TFrom>(TFrom from)
            where TFrom : class
        {
            return InsertOrUpdate(typeof(TFrom), from);
        }

        public TTo InsertOrUpdate(Type type, object from)
        {
            var equivExpr = _mapper == null
                ? Mapper.Map(from, type, typeof(Expression<Func<TTo, bool>>)) as Expression<Func<TTo, bool>>
                : _mapper.Map(from, type, typeof(Expression<Func<TTo, bool>>)) as Expression<Func<TTo, bool>>;
            if (equivExpr == null)
                throw new ArgumentException($"Could not retreive equivalency expression for mapping {type.Name} --> {typeof(TTo).Name}");

            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to == null)
            {
                to = (TTo)(_mapper?.Map(from, type, typeof(TTo)) ?? Mapper.Map(from, type, typeof(TTo)));
                _sourceSet.Add(to);
            }
            else
            {
                if (_mapper == null)
                    Mapper.Map(from, to);
                else
                    _mapper.Map(from, to);
            }

            return to;
        }

        public void Remove<TFrom>(TFrom from)
            where TFrom : class
        {
            var equivExpr = _mapper == null
                ? Mapper.Map<TFrom, Expression<Func<TTo, bool>>>(from)
                : _mapper.Map<TFrom, Expression<Func<TTo, bool>>>(from);
            if (equivExpr == null)
                return;
            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to != null)
                _sourceSet.Remove(to);
        }
    }
}