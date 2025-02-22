using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.IRepositories;

namespace SabiMarket.Infrastructure.Utilities
{
    public class CustomIdGenerator
    {
        private readonly IRepositoryManager _repositoryManager;
        public CustomIdGenerator(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<string> GenerateIdAsync(string companyId)
        {
            var staff = await _repositoryManager.SowFoodCompanyStaffRepository.GetLastCompanyStaff(companyId, true);
            if (staff is null)
            {
                return $"ID000001";
            }

            var lastId = staff.StaffId;

            var numericPart = lastId.Substring(2);

            if (!int.TryParse(numericPart, out int number))
            {
                throw new InvalidOperationException("Invalid numeric format in Staff ID");
            }

            number++; // Increment the number

            return $"ID{number.ToString("D6")}";
        }
    }
}
