using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SistemaWebPapeleria.PruebasUnitarias
{
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _datos = new();

        public string Id => "sesion-de-prueba";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _datos.Keys;

        public void Clear() => _datos.Clear();
        public void Remove(string key) => _datos.Remove(key);
        public void Set(string key, byte[] value) => _datos[key] = value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
            => _datos.TryGetValue(key, out value);

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
