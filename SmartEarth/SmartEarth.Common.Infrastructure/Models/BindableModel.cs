using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Models
{
    public class BindableModel
    {
        #region Properties
        /// <summary>
        /// This event is called when a property is changed.
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (expression == null) return;
            var body = (MemberExpression)expression.Body;

            if (body == null) return;
            RaisePropertyChanged(body.Member.Name);
        }

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> expression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(expression);
            return true;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
