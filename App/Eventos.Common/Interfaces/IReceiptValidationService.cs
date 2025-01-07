using System;

namespace Eventos.Common.Interfaces;

public interface IReceiptValidationService
{
    Task<bool> ValidateReceiptAsync(string receiptData);

}
