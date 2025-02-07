using System;

namespace TaskManagementSystem.API.Services
{
    public class TaskPriorityService
    {
        // Constants for the formula
        private const double CategoryWeight = 0.6;   // wc
        private const double UserWeight = 0.4;       // wu
        private const double TimeSensitivity = 2.5;  // k

        /// <summary>
        /// Calculates the task's calculated priority value.
        /// </summary>
        /// <param name="categoryPriority">The priority value defined on the Category (1–4).</param>
        /// <param name="userAssignedPriority">The priority value provided by the user (1–5).</param>
        /// <param name="dueDate">The due date of the task.</param>
        /// <returns>An integer priority value on a 0–100 scale.</returns>
        public int CalculateTaskPriority(int categoryPriority, int userAssignedPriority, DateTime dueDate)
        {
            // Normalize the category and user priorities
            double normalizedCategory = categoryPriority / 4.0; // value between 0.25 and 1.0
            double normalizedUser = userAssignedPriority / 5.0;   // value between 0.2 and 1.0

            // Calculate time difference in hours, ensuring a minimum to avoid log(0)
            TimeSpan timeLeft = dueDate - DateTime.UtcNow;
            double hoursLeft = Math.Max(timeLeft.TotalHours, 0.1);    

            // Calculate the time sensitivity factor using a logarithmic scale
            double timeFactor = 1 + (TimeSensitivity / Math.Log(hoursLeft + 1));

            // Combine the factors (weighted average then adjusted by time sensitivity)
            double combinedValue = ((normalizedCategory * CategoryWeight) + (normalizedUser * UserWeight)) / (CategoryWeight + UserWeight);
            double calculatedPriority = combinedValue * timeFactor;

            // Scale result to 0-100
            return (int)Math.Round(calculatedPriority * 100, MidpointRounding.AwayFromZero);
        }
    }
}
