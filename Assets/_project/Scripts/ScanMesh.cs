﻿using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScanMesh : MonoBehaviour
{
    [SerializeField] private GameObject _meshPrefab; // Префаб для отображения меша
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;

    [SerializeField] private RawImage rawImage;
    [SerializeField] private RawImage rawFrameImage;

    private List<MeshRenderer> _meshes = new List<MeshRenderer>();
    private Texture2D cameraTexture;

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
        _arCameraManager.frameReceived += _arCameraManager_frameReceived;
    }

    private void _arCameraManager_frameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (eventArgs.textures.Count > 0)
        {
            // Получаем первую текстуру из массива
            cameraTexture = eventArgs.textures[0];
            // Поворачиваем текстуру по часовой стрелке на 90 градусов
            cameraTexture = RotateTextureClockwise(cameraTexture);

            // Отражаем текстуру по горизонтали
            cameraTexture = FlipTextureHorizontally(cameraTexture);

            rawFrameImage.texture = cameraTexture;
        }
        else
        {
            Debug.Log("Текстур нет");
        }
    }

    private Texture2D RotateTextureClockwise(Texture2D originalTexture)
    {
        // Создаем новую текстуру с измененными размерами и форматом пикселей
        Texture2D rotatedTexture = new Texture2D(originalTexture.height, originalTexture.width, originalTexture.format, false);

        // Поворачиваем каждый пиксель текстуры
        for (int x = 0; x < originalTexture.width; x++)
        {
            for (int y = 0; y < originalTexture.height; y++)
            {
                rotatedTexture.SetPixel(y, originalTexture.width - x - 1, originalTexture.GetPixel(x, y));
            }
        }

        // Применяем изменения
        rotatedTexture.Apply();

        return rotatedTexture;
    }

    private Texture2D FlipTextureHorizontally(Texture2D originalTexture)
    {
        // Создаем новую текстуру с теми же размерами и форматом пикселей
        Texture2D flippedTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, false);

        // Отражаем каждый пиксель текстуры по горизонтали
        for (int x = 0; x < originalTexture.width; x++)
        {
            for (int y = 0; y < originalTexture.height; y++)
            {
                flippedTexture.SetPixel(originalTexture.width - x - 1, y, originalTexture.GetPixel(x, y));
            }
        }

        // Применяем изменения
        flippedTexture.Apply();

        return flippedTexture;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
    }

    private void OnDestroy()
    {
        Destroy(cameraTexture);
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshFilter in eventArgs.added)
        {
            CreateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.updated)
        {
            UpdateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.removed)
        {
            RemoveMeshObject(meshFilter);
        }
    }

    private void CreateMeshObject(MeshFilter meshFilter)
    {
        // Получение вершин меша
        Vector3[] vertices = meshFilter.mesh.vertices;

        // Получение треугольников меша
        int[] triangles = meshFilter.mesh.triangles;

        // Создание текстурных координат для меша
        Vector2[] uvs = GenerateUVs(vertices);

        GameObject meshObject = Instantiate(_meshPrefab, Vector3.zero, Quaternion.identity);

        // Передача данных меша объекту
        Mesh meshComponent = new Mesh();
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshObject.GetComponent<MeshFilter>().mesh = meshComponent;

        // Расположение объекта в пространстве
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;
        _meshes.Add(meshObject.GetComponent<MeshRenderer>());
        // Применяем текстуру к мешу
        ApplyCameraTextureToMesh(meshObject, meshFilter);
    }

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        // Получение вершин меша
        Vector3[] vertices = meshFilter.sharedMesh.vertices;

        // Получение треугольников меша
        int[] triangles = meshFilter.sharedMesh.triangles;

        // Получение текстурных координат меша
        Vector2[] uvs = meshFilter.sharedMesh.uv;

        // Обновление существующего меша
        GameObject meshObject = meshFilter.gameObject;
        Mesh meshComponent = meshObject.GetComponent<MeshFilter>().mesh;
        meshComponent.Clear(); // Очищаем меш перед обновлением
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshComponent.RecalculateNormals(); // Пересчитываем нормали

        // Расположение объекта в пространстве (если требуется)
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;

        // Применяем текстуру к мешу
        ApplyCameraTextureToMesh(meshObject, meshFilter);
    }

    private void RemoveMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        Destroy(meshFilter.gameObject);
    }

     private Texture2D GetCameraTextureForMesh(MeshFilter meshFilter)
    {
        rawImage.enabled = false;
        if (_arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // Получаем размеры меша
            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            Vector3 meshSize = meshBounds.size;

            // Преобразуем размеры меша в нормализованные координаты от 0 до 1
            float normalizedWidth = meshSize.x / meshBounds.extents.x;
            float normalizedHeight = meshSize.z / meshBounds.extents.z;

            // Получаем прямоугольник, соответствующий размерам меша
            Rect meshRect = new Rect(0, 0, normalizedWidth, normalizedHeight);

            // Вычисляем размеры области кадра, соответствующей мешу
            // Определяем размеры области кадра, соответствующей мешу
            int imageWidth = image.width;
            int imageHeight = image.height;
            int x = Mathf.FloorToInt(meshRect.x * image.width);
            int y = Mathf.FloorToInt(meshRect.y * image.height);
            int width = Mathf.CeilToInt(meshRect.width * image.width);
            int height = Mathf.CeilToInt(meshRect.height * image.height);

            // Проверяем, чтобы размеры преобразованного изображения не превышали размеры оригинального изображения
            if (x + width > imageWidth)
            {
                width = imageWidth - x;
            }

            if (y + height > imageHeight)
            {
                height = imageHeight - y;
            }

            // Создаем новую текстуру с размерами меша
            Texture2D cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Определяем размер буфера для преобразования
            int bufferSize = image.GetConvertedDataSize(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(x, y, width, height),
                outputDimensions = new Vector2Int(width, height),
                outputFormat = TextureFormat.RGBA32
            });

            // Создаем буфер для преобразования
            NativeArray<byte> buffer = new NativeArray<byte>(bufferSize, Allocator.Temp);

            // Выполняем преобразование
            image.Convert(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(x, y, width, height),
                outputDimensions = new Vector2Int(width, height),
                outputFormat = TextureFormat.RGBA32
            }, buffer);

            // Копируем данные из буфера в текстуру
            cameraTexture.LoadRawTextureData(buffer);
            cameraTexture.Apply();

            // Освобождаем ресурсы
            image.Dispose();
            buffer.Dispose();
            _meshes.ForEach(mesh =>
            {
                if (mesh != null)
                    mesh.enabled = true;
            });
            rawImage.enabled = true;
            return cameraTexture;
        }
        rawImage.enabled = true;
        return null;
    }

    private Texture2D ColorMeshWithCameraTexture(MeshFilter meshFilter, Texture2D cameraTexture)
    {
        // Получаем меш и его размеры
        Mesh mesh = meshFilter.sharedMesh;
        Bounds meshBounds = mesh.bounds;
        Vector3 meshSize = meshBounds.size;

        // Создаем новую текстуру с размерами меша
        Texture2D meshTexture = new Texture2D((int)meshSize.x, (int)meshSize.y, TextureFormat.RGBA32, false);

        // Получаем вершины меша в локальных координатах
        Vector3[] meshVertices = mesh.vertices;

        // Проходим по вершинам и преобразуем их в координаты текстуры
        Vector2[] meshUVs = new Vector2[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i++)
        {
            Vector3 vertex = meshVertices[i];
            Vector3 normalizedVertex = new Vector3(
                (vertex.x - meshBounds.min.x) / meshSize.x,
                (vertex.y - meshBounds.min.y) / meshSize.y,
                (vertex.z - meshBounds.min.z) / meshSize.z
            );
            meshUVs[i] = new Vector2(normalizedVertex.x, normalizedVertex.y);
        }

        // Проходим по треугольникам меша и копируем пиксели из соответствующих областей на кадре в текстуру меша
        int[] meshTriangles = mesh.triangles;
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            Vector2 uv1 = meshUVs[meshTriangles[i]];
            Vector2 uv2 = meshUVs[meshTriangles[i + 1]];
            Vector2 uv3 = meshUVs[meshTriangles[i + 2]];

            int minX = Mathf.FloorToInt(Mathf.Min(uv1.x, uv2.x, uv3.x) * cameraTexture.width);
            int minY = Mathf.FloorToInt(Mathf.Min(uv1.y, uv2.y, uv3.y) * cameraTexture.height);
            int maxX = Mathf.CeilToInt(Mathf.Max(uv1.x, uv2.x, uv3.x) * cameraTexture.width);
            int maxY = Mathf.CeilToInt(Mathf.Max(uv1.y, uv2.y, uv3.y) * cameraTexture.height);

            Color[] pixels = cameraTexture.GetPixels(minX, minY, maxX - minX, maxY - minY);

            // Получаем индексы вершин треугольника в массиве треугольников
            int index1 = meshTriangles[i];
            int index2 = meshTriangles[i + 1];
            int index3 = meshTriangles[i + 2];

            // Задаем UV-координаты для треугольника
            meshUVs[index1] = new Vector2((uv1.x - minX) / (maxX - minX), (uv1.y - minY) / (maxY - minY));
            meshUVs[index2] = new Vector2((uv2.x - minX) / (maxX - minX), (uv2.y - minY) / (maxY - minY));
            meshUVs[index3] = new Vector2((uv3.x - minX) / (maxX - minX), (uv3.y - minY) / (maxY - minY));

            // Задаем цвета вершин треугольника на текстуре меша
            meshTexture.SetPixels((int)meshUVs[index1].x, (int)meshUVs[index1].y, 1, 1, pixels);
            meshTexture.SetPixels((int)meshUVs[index2].x, (int)meshUVs[index2].y, 1, 1, pixels);
            meshTexture.SetPixels((int)meshUVs[index3].x, (int)meshUVs[index3].y, 1, 1, pixels);
        }

        // Применяем изменения на текстуре меша
        meshTexture.Apply();

        // Устанавливаем текстуру меша в материале
        return meshTexture;
    }


    private void ApplyCameraTextureToMesh(GameObject meshObject, MeshFilter meshFilter)
    {
        ToogleMeshes(false);
        if (_arCameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            //cameraTexture = GetCameraTextureForMesh(meshObject.GetComponent<MeshFilter>());
            var texture = ColorMeshWithCameraTexture(meshObject.GetComponent<MeshFilter>(), cameraTexture);
            cpuImage.Dispose();
            //Texture2D cameraTexture = GetCameraTextureForMesh(meshObject.GetComponent<MeshFilter>());
            //
            rawImage.texture = texture;

            if (texture != null)
            {
                // Создаем новый материал для меша
                Material material = new Material(Shader.Find("Standard"));

                // Присваиваем текстуру новому материалу
                material.mainTexture = texture;

                //// Изменяем ориентацию текстуры
                //material.mainTextureScale = new Vector2(-1, 1); // Изменяем знаки X-координаты и Y-координаты

                // Применяем новый материал к мешу
                meshObject.GetComponent<MeshRenderer>().material = material;
            }
        }
        ToogleMeshes(true);
    }

    private Texture2D CreateTextureFromCpuImage(XRCpuImage cpuImage)
    {
        // Создаем новую текстуру с размерами кадра
        Texture2D texture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);

        // Копируем данные изображения в текстуру
        ConvertCpuImageToTexture(cpuImage, texture);

        return texture;
    }

    private void ConvertCpuImageToTexture(XRCpuImage cpuImage, Texture2D texture)
    {
        // Получаем данные изображения
        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32);
        var conversionJob = cpuImage.ConvertAsync(conversionParams);

        // Ожидаем завершения конвертации
        while (!conversionJob.status.IsDone())
        {
            // Ждем завершения конвертации
        }

        // Копируем данные в текстуру
        conversionJob.GetData<byte>().CopyTo(texture.GetRawTextureData());

        // Применяем изменения на текстуре
        texture.Apply();
    }

    private void ToogleMeshes(bool activate)
    {
        if (_meshes == null || _meshes.Count <= 0)
            return;

        _meshes.ForEach(mesh =>
        {
            if (mesh != null)
                mesh.enabled = activate;
        });
    }

    private Vector2[] GenerateUVs(Vector3[] vertices)
    {
        // Создаем массив текстурных координат для каждой вершины меша
        Vector2[] uvs = new Vector2[vertices.Length];

        // Проходим по каждой вершине и создаем текстурную координату,
        // основанную на позиции вершины в пространстве
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector2 uv = new Vector2(vertex.x, vertex.z);
            uvs[i] = uv;
        }

        return uvs;
    }
}