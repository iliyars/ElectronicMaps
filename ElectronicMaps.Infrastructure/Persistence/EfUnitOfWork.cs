using DocumentFormat.OpenXml.Office2016.Excel;
using ElectronicMaps.Application.Abstractions.Persistence;
using ElectronicMaps.Domain.Exceptions;
using ElectronicMaps.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Persistence
{
    public class EfUnitOfWork : IUnitOfWork
    {

        private readonly AppDbContext _db;
        private readonly ILogger<EfUnitOfWork> _logger;

        public EfUnitOfWork(AppDbContext db, ILogger<EfUnitOfWork> logger)
        {
            _db = db;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
        {
            // Стратегия повторных попыток для обработки временных сбоев
            var strategy = _db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    _logger.LogDebug("Транзакция начата");

                    await action(ct);

                    await transaction.CommitAsync(ct);

                    _logger.LogDebug("Транзакция успешно завершена");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                       ex,
                       "Транзакция откатывается из-за ошибки: {ErrorMessage}",
                       ex.Message);

                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct)
        {
            try
            {
                var changeCount = await _db.SaveChangesAsync(ct);

                _logger.LogDebug(
                    "SaveChanges выполнен успешно. Изменено записей: {ChangeCount}",
                    changeCount);

                return changeCount;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка конкурентности при сохранении изменений");

                throw new ConcurrencyException(
                    "Данные были изменены другим пользователем. Обновите данные и попробуйте снова.",
                    ex);
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка базы данных при сохранении изменений");

                throw new DataAccessException(
                    "Не удалось сохранить изменения в базу данных. Проверьте корректность данных.",
                    ex);
            }
        }


    }
}
