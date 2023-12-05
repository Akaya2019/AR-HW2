using UnityEngine;

public class RansacSolver
{
    public static void Ransac(Vector3[] pointsP, Vector3[] pointsQ, int numIterations, int subsetSize,
                              out Vector3[] bestSubsetP, out Vector3[] bestSubsetQ)
    {
        bestSubsetP = null;
        bestSubsetQ = null;

        for (int iteration = 0; iteration < numIterations; iteration++)
        {
            // Randomly select a subset of points
            int[] randomIndices = GetRandomSubsetIndices(pointsP.Length, subsetSize);
            Vector3[] subsetP = GetSubset(pointsP, randomIndices);
            Vector3[] subsetQ = GetSubset(pointsQ, randomIndices);

            // Check if the subset is a good candidate
            if (IsGoodCandidate(subsetP, subsetQ))
            {
                bestSubsetP = subsetP;
                bestSubsetQ = subsetQ;
                break; // Exit early if a good subset is found
            }
        }
    }

    private static int[] GetRandomSubsetIndices(int totalPoints, int subsetSize)
    {
        // Generate random indices for the subset
        int[] indices = new int[subsetSize];
        for (int i = 0; i < subsetSize; i++)
        {
            indices[i] = Random.Range(0, totalPoints);
        }

        return indices;
    }

    private static Vector3[] GetSubset(Vector3[] points, int[] indices)
    {
        // Extract a subset of points using the given indices
        Vector3[] subset = new Vector3[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            subset[i] = points[indices[i]];
        }

        return subset;
    }

    private static bool IsGoodCandidate(Vector3[] subsetP, Vector3[] subsetQ)
    {
        // Implement a criterion to determine if the subset is a good candidate
        // For simplicity, you may use a distance-based criterion or other criteria based on your specific needs.
        // For example, you could calculate the average distance between corresponding points in the subsets
        float threshold = 3.0f; // Adjust the threshold based on your requirements

        float averageDistance = 0.0f;
        for (int i = 0; i < subsetP.Length; i++)
        {
            averageDistance += Vector3.Distance(subsetP[i], subsetQ[i]);
        }

        averageDistance /= subsetP.Length;

        return averageDistance < threshold;
    }
}
