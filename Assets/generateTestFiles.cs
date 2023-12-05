using System.IO;
using UnityEngine;

public class GenerateTestFiles : MonoBehaviour
{
    private const int NumPoints = 10;

    public void OnStartClick()
    {
        // Rigid d�n���m matrisi
        Matrix4x4 rigidTransform = Matrix4x4.TRS(Vector3.up * 2, Quaternion.Euler(0, 30, 0), Vector3.one);

        // Global �l�ekleme d�n���m matrisi
        Matrix4x4 globalScaleTransform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(2, 0.5f, 1.5f));

        // Dosya yollar�
        string filePathP = "Assets/FileP.txt";
        string filePathQ = "Assets/FileQ.txt";

        // Dosyalara matrislerle doldurulmu� noktalar� yaz
        WritePointsToFile(filePathP, rigidTransform);
        WritePointsToFile(filePathQ, globalScaleTransform);
    }

    private void WritePointsToFile(string filePath, Matrix4x4 transformationMatrix)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(NumPoints);

            for (int i = 0; i < NumPoints; i++)
            {
                Vector3 originalPoint = Random.insideUnitSphere * 5; // Rastgele bir nokta olu�tur

                // D�n���m� uygula
                Vector3 transformedPoint = transformationMatrix.MultiplyPoint3x4(originalPoint);

                // Dosyaya yaz
                writer.WriteLine($"{transformedPoint.x} {transformedPoint.y} {transformedPoint.z}");
            }
        }
    }
}
