using Core.Utilities.Results;

namespace Core.Utilities.Business;

public class BusinessRules
{
    public static async Task<IResult> Run(params Task<IResult>[] logics)
    {
        foreach (var logic in logics)
        {
            var result = await logic;
            if (!result.Success)
            {
                return result;
            }
        }
        return null;
    }
}