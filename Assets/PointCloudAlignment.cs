using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PointCloudAlignment;

public class PointCloudAlignment : MonoBehaviour
{
    public enum TransformationType
    {
        Rigid,
        RigidGlobalScale
    }

    public Text transformationParametersText; // Bu UI elemanýný Inspector üzerinden baðlamayý unutma
    //public Transform pointCloudParent; // Noktalarýn parent'ý olacak transform
    public Transform pointCloudParentP; // P noktalarýnýn parent'ý olacak transform
    public Transform pointCloudParentPP; // P noktalarýnýn parent'ý olacak transform
    public Transform pointCloudParentQ; // Hizalanmýþ Q noktalarýnýn parent'ý olacak transform
    public Transform pointCloudParentTransformedP; // Dönüþtürülmüþ P noktalarýnýn parent'ý olacak transform
    public Transform lineParent; // Çizgilerin parent'ý olacak transform


    public Vector3[] pointsP, pointsQ;
    public Vector3[] bestSubsetPP, bestSubsetQQ;
    public Matrix4x4 translationMatrix;
    public Matrix4x4 scaleMatrix;
    public Matrix4x4 rotationMatrix;


    // Unity UI buttons callback methods
    public void OnStartButtonClick()
    {
        if (LoadPointClouds())
        {
            // Update visualization
            UpdateVisualization(false);
        }
    }

    public void OnRigidTransformButtonClick()
    {
        clearr();
        // Implement rigid transformation
        Ransac(pointsP, pointsQ, 10000, 3, TransformationType.Rigid,
                              out Vector3[] bestSubsetP, out Vector3[] bestSubsetQ);
        bestSubsetPP = bestSubsetP;
        bestSubsetQQ = bestSubsetQ;
        //Debug.Log($"bestSubsetPP length: \n{bestSubsetPP.Length}");
        //Debug.Log($"bestSubsetQQ length: \n{bestSubsetQQ.Length}");
        Matrix4x4 transformationMatrix = CalculateRigidTransformationMatrix(bestSubsetPP, bestSubsetQQ);
        ApplyTransformation(transformationMatrix);
        //lineDrawer();

        // Update visualization
        UpdateVisualization();

        // Display transformation parameters
        DisplayTransformationParameters(transformationMatrix, TransformationType.Rigid);
    }

    public void OnScaleTransformButtonClick()
    {
        clearr();
        // Implement scale transformation
        Ransac(pointsP, pointsQ, 10000, 3, TransformationType.RigidGlobalScale,
                      out Vector3[] bestSubsetP, out Vector3[] bestSubsetQ);
        bestSubsetPP = bestSubsetP;
        bestSubsetQQ = bestSubsetQ;
        Matrix4x4 transformationMatrix = CalculateGlobalScaleTransformationMatrix(bestSubsetPP, bestSubsetQQ);
        ApplyTransformation(transformationMatrix);

        // Update visualization
        UpdateVisualization();

        // Display transformation parameters
        DisplayTransformationParameters(transformationMatrix, TransformationType.RigidGlobalScale);
    }

    public void OnDrawRTClick()
    {
        lineDrawer();
    }
    public void OnDrawRTGSClick()
    {
        lineDrawer();
    }
    public void OnClearRTlick() 
    {
        foreach (Transform child in lineParent)
        {
            Destroy(child.gameObject);
        }
    }
    public void OnClearRTGSlick()
    {
        foreach (Transform child in lineParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void clearr()
    {
        // Önceki görselleþtirmeleri temizle
        foreach (Transform child in pointCloudParentP)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pointCloudParentQ)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pointCloudParentTransformedP)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pointCloudParentPP)
        {
            Destroy(child.gameObject);
        }
        // Önceki çizgileri temizle
        foreach (Transform child in lineParent)
        {
            Destroy(child.gameObject);
        }
    }


    // Nokta bulutlarýný yükle
    private bool LoadPointClouds()
    {
        // Dosya yollarýný belirt
        string filePathP = "C:\\Users\\Ali Kaya\\unity-basics-and-ar\\Assets\\FileP.txt"; // P nokta bulutu dosyasý yolu
        string filePathQ = "C:\\Users\\Ali Kaya\\unity-basics-and-ar\\Assets\\FileQ.txt"; // Q nokta bulutu dosyasý yolu

        // Nokta bulutu dosyalarýný yükle
        bool successP = FileLoader.LoadPointCloudFile(filePathP, out pointsP);
        bool successQ = FileLoader.LoadPointCloudFile(filePathQ, out pointsQ);
        //Debug.Log($"PointsP length:\n{pointsP.Length}");
        //Debug.Log($"PointsQ length:\n{pointsQ.Length}");

        if (!successP)
        {
            Debug.LogError("Failed to load point cloud from file P.");
        }

        if (!successQ)
        {
            Debug.LogError("Failed to load point cloud from file Q.");
        }

        return successP && successQ;
    }

    // Rigid dönüþüm matrisini hesapla
    private Matrix4x4 CalculateRigidTransformationMatrix(Vector3[] subsetP, Vector3[] subsetQ)
    {
        if (subsetP.Length != subsetQ.Length || subsetP.Length < 3)
        {
            Debug.LogError("Invalid subset sizes for rigid transformation calculation.");
            return Matrix4x4.identity;
        }

        // Bu örnekte, Unity'deki MathUtils fonksiyonlarýný kullanarak dönüþüm matrisi hesapla
        Vector3 centroidP = CalculateCentroid(subsetP);
        Vector3 centroidQ = CalculateCentroid(subsetQ);

        translationMatrix = Matrix4x4.Translate(centroidQ - centroidP);
        rotationMatrix = CalculateRotationMatrix(subsetP, subsetQ);

        //Debug.Log($"Translation Matrix:\n{translationMatrix}");
        //Debug.Log($"Rotation Matrix:\n{rotationMatrix}");

        return translationMatrix * rotationMatrix;
    }

    // Global ölçekleme dönüþüm matrisini hesapla
    private Matrix4x4 CalculateGlobalScaleTransformationMatrix(Vector3[] subsetP, Vector3[] subsetQ)
    {
        if (subsetP.Length != subsetQ.Length || subsetP.Length < 3)
        {
            Debug.LogError("Invalid subset sizes for global scale transformation calculation.");
            return Matrix4x4.identity;
        }

        // Bu örnekte, Unity'deki MathUtils fonksiyonlarýný kullanarak dönüþüm matrisi hesapla
        Vector3 centroidP = CalculateCentroid(subsetP);
        Vector3 centroidQ = CalculateCentroid(subsetQ);

        Vector3 scale = new Vector3(
            Vector3.Distance(subsetQ[0], centroidQ) / Vector3.Distance(subsetP[0], centroidP),
            Vector3.Distance(subsetQ[1], centroidQ) / Vector3.Distance(subsetP[1], centroidP),
            Vector3.Distance(subsetQ[2], centroidQ) / Vector3.Distance(subsetP[2], centroidP)
        );

        translationMatrix = Matrix4x4.Translate(centroidQ - centroidP);
        rotationMatrix = CalculateRotationMatrix(subsetP, subsetQ);
        scaleMatrix = Matrix4x4.Scale(scale);

        //Debug.Log($"Translation Matrix:\n{translationMatrix}");
        //Debug.Log($"Rotation Matrix:\n{rotationMatrix}");
        //Debug.Log($"Scale Matrix:\n{scaleMatrix}");

        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    // Nokta bulutunun merkezini hesapla
    private Vector3 CalculateCentroid(Vector3[] points)
    {
        Vector3 centroid = Vector3.zero;

        foreach (Vector3 point in points)
        {
            centroid += point;
        }

        centroid /= points.Length;

        return centroid;
    }

    // Noktalar arasýndaki dönüþüm matrisini hesapla
    private Matrix4x4 CalculateRotationMatrix(Vector3[] subsetP, Vector3[] subsetQ)
    {
        // Ýki nokta seti arasýndaki çapraz çarpýmý hesapla
        Vector3 cross = Vector3.Cross(subsetP[0], subsetQ[0]);

        // Ýki nokta seti arasýndaki açýyý hesapla
        float angle = Vector3.Angle(subsetP[0], subsetQ[0]);

        // Döndürme matrisini oluþtur
        Quaternion rotation = Quaternion.AngleAxis(angle, cross);
        return Matrix4x4.Rotate(rotation);
    }


    private void ApplyTransformation(Matrix4x4 transformationMatrix)
    {
        for (int i = 0; i < bestSubsetPP.Length; i++)
        {
            bestSubsetPP[i] = transformationMatrix.MultiplyPoint3x4(bestSubsetPP[i]);
        }
    }


    // Görselleþtirmeyi güncelle
    private void UpdateVisualization(bool flag = true)
    {
        // Önceki görselleþtirmeleri temizle
        foreach (Transform child in pointCloudParentP)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pointCloudParentQ)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in pointCloudParentTransformedP)
        {
            Destroy(child.gameObject);
        }

        // Yeni görselleþtirmeyi oluþtur
        PointCloudVisualizer visualizer = FindObjectOfType<PointCloudVisualizer>();
        if (!flag)
        {
            visualizer.VisualizePoints(pointsP, Color.red, pointCloudParentP); // P noktalarýný kýrmýzý renkte görselleþtir
            visualizer.VisualizePoints(pointsQ, Color.blue, pointCloudParentQ); // Q noktalarýný kýrmýzý renkte görselleþtir
        }
        else
        {
            //visualizer.VisualizePoints(bestSubsetPP, Color.green); // Dönüþüm uygulanan P nokta bulutunu yeþil renkte görselleþtir
            visualizer.VisualizePoints(pointsP, Color.red, pointCloudParentP); // P noktalarýný kýrmýzý renkte görselleþtir
            visualizer.VisualizePoints(bestSubsetQQ, Color.green, pointCloudParentTransformedP); // Q noktalarýný kýrmýzý renkte görselleþtir                                                       // 
            visualizer.VisualizePoints(pointsQ, Color.blue, pointCloudParentQ); // Q noktalarýný kýrmýzý renkte görselleþtir
        }
    }

    private void lineDrawer()
    {
        // Önceki çizgileri temizle
        foreach (Transform child in lineParent)
        {
            Destroy(child.gameObject);
        }

        PointCloudVisualizer visualizer = FindObjectOfType<PointCloudVisualizer>();
        visualizer.VisualizePoints(bestSubsetPP, Color.black, pointCloudParentPP); // PP noktalarýný kýrmýzý renkte görselleþtir
                                                                                   // Her bir noktanýn hareketini bir çizgi olarak göster
        for (int i = 0; i < bestSubsetPP.Length; i++)
        {
            GameObject lineObject = new GameObject("Line" + i);
            lineObject.transform.parent = lineParent;
            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

            // Çizgi kalýnlýðýný ayarla
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            // Çizgi rengini ayarla
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;

            // Çizgi pozisyonlarýný ve sayýsýný ayarla
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, pointsP[i]);
            lineRenderer.SetPosition(1, bestSubsetPP[i]);

            // Dünya koordinatlarýný kullan
            lineRenderer.useWorldSpace = true;

            // Çizgi malzemesini ayarla
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }



    // Dönüþüm parametrelerini ekranda göster
    private void DisplayTransformationParameters(Matrix4x4 transformationMatrix, TransformationType transformationType)
    {
        // Dönüþüm parametrelerini ekranda göster
        if (transformationType == TransformationType.Rigid)
        {
            transformationParametersText.text = $"Transformation Matrix:\n{transformationMatrix}\n" +
                                                $"Rotation Matrix:\n{rotationMatrix}\n" +
                                                $"Translation Matrix:\n{translationMatrix}";
        }
        else
        {
            transformationParametersText.text = $"Transformation Matrix:\n{transformationMatrix}\n" +
                                    $"Scale Matrix:\n{scaleMatrix}\n" +
                                    $"Rotation Matrix:\n{rotationMatrix}\n" +
                                    $"Translation Matrix:\n{translationMatrix}";
        }

        //Debug.Log($"Displayed Transformation Matrix:\n{transformationMatrix}");
    }


    public void Ransac(Vector3[] pointsP, Vector3[] pointsQ, int numIterations, int subsetSize, TransformationType transformationType,
                              out Vector3[] bestSubsetP, out Vector3[] bestSubsetQ)
    {
        bestSubsetP = null;
        bestSubsetQ = null;
        float bestError = float.MaxValue;

        for (int iteration = 0; iteration < numIterations; iteration++)
        {
            // Randomly select a subset of points
            Vector3[] subsetP = SelectRandomSubset(pointsP, subsetSize);
            Vector3[] subsetQ = SelectRandomSubset(pointsQ, subsetSize);
            Matrix4x4 transformation;
            if (transformationType == TransformationType.Rigid)
            {
                transformation = CalculateRigidTransformationMatrix(subsetP, subsetQ);
            }
            else
            {
                transformation = CalculateGlobalScaleTransformationMatrix(subsetP, subsetQ);
            }

            float error = CalculateError(pointsP, pointsQ, transformation);

            if (error < bestError)
            {
                bestError = error;
                bestSubsetP = subsetP;
                bestSubsetQ = subsetQ;
            }
        }
    }

    private static Vector3[] SelectRandomSubset(Vector3[] points, int subsetSize)
    {
        int[] indices = new int[subsetSize];
        for (int i = 0; i < subsetSize; i++)
        {
            indices[i] = UnityEngine.Random.Range(0, points.Length);
        }

        Vector3[] subset = new Vector3[subsetSize];
        for (int i = 0; i < subsetSize; i++)
        {
            subset[i] = points[indices[i]];
        }

        return subset;
    }

    public static float CalculateError(Vector3[] pointsP, Vector3[] pointsQ, Matrix4x4 transformation)
    {
        float totalError = 0f;
        for (int i = 0; i < pointsP.Length; i++)
        {
            Vector3 transformedPoint = transformation.MultiplyPoint3x4(pointsP[i]);
            totalError += Vector3.Distance(transformedPoint, pointsQ[i]);
        }
        return totalError / pointsP.Length;
    }

}