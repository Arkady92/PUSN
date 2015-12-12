#include "heightMap.h"
using namespace std;
using namespace gk2;

const unsigned int HeightMap::VB_STRIDE = sizeof(VertexPosNormal);
const unsigned int HeightMap::VB_OFFSET = 0;

HeightMap::HeightMap(Service& service, int _gridWidth, int _gridHeight, int _width, int _height, int _minY)
{
	m_service = service;
	gridWidth = _gridWidth;
	gridHeight = _gridHeight;
	height = _height;
	width = _width;
	minY = _minY;
	Initialize();
}


HeightMap::HeightMap(const HeightMap& other)
{
}


HeightMap::~HeightMap()
{
	m_Vertexes.reset();
}



void HeightMap::InitializeMap(int n, int m)
{
	// Create the structure to hold the height map data.
	m_heightMap = new XMFLOAT3[n * m];
	//m_lowMap = new XMFLOAT3[2 * (n + m) - 4];
	m_lowMap = new XMFLOAT3[n * m];

	int lowIndex = 0;
	// Read the image data into the height map.
	for (int j = 0; j < n; j++)
	{
		for (int i = 0; i < m; i++)
		{
			float x = (((float)i / (float)(m - 1)) - 0.5) * width / 100;
			float size = 50.0f;
			float z = ((float)j / (float)(n - 1) - 0.5) * height / 100;

			int index = (n * j) + i;

			m_heightMap[index].x = x;
			m_heightMap[index].y = size / 100;
			m_heightMap[index].z = z;

			float low_heigth = 0;
			m_lowMap[lowIndex].x = x;
			m_lowMap[lowIndex].y = low_heigth / 100;
			m_lowMap[lowIndex].z = z;
			lowIndex++;
		}
	}
	int a = 10;
}


void HeightMap::Initialize()
{
	InitializeMap(gridWidth, gridHeight);

	verticesCount = gridWidth * gridHeight * 2;
	indicesMeshCount = (gridWidth * gridHeight + 4 * gridHeight) * 12;
	indicesMaterialCount = gridWidth * gridHeight * 6;

	cubeVertices = new VertexPosNormal[verticesCount];
	cubeIndicesMesh = new unsigned short[indicesMeshCount];
	cubeIndicesMaterial = new unsigned short[indicesMaterialCount];

	// Initialize the index to the vertex buffer.
	int index = 0;

	for (int j = 0; j < gridHeight; j++)
	{
		for (int i = 0; i < gridWidth; i++)
		{
			cubeVertices[index].Pos = m_heightMap[index];
			cubeVertices[index].Normal = XMFLOAT3(0.0, 1.0, 0.0);
			index++;
		}
	}

	int k = 0;

	for (int j = 0; j < gridHeight; j++)
	{
		for (int i = 0; i < gridWidth; i++)
		{
			cubeVertices[index].Pos = m_lowMap[k++];
			cubeVertices[index].Normal = XMFLOAT3(0.0, 1.0, 0.0);
			index++;
		}
	}

	index = 0;
	for (int j = 0; j < (gridHeight - 1); j++)
	{
		for (int i = 0; i < (gridWidth - 1); i++)
		{
			int	index1 = (gridHeight * j) + i;          // Bottom left.
			int index2 = (gridHeight * j) + (i + 1);      // Bottom right.
			int index3 = (gridHeight * (j + 1)) + i;      // Upper left.
			int index4 = (gridHeight * (j + 1)) + (i + 1);  // Upper right.

			// Upper left.
			cubeIndicesMaterial[index] = index1;
			index++;

			// Upper Right.
			cubeIndicesMaterial[index] = index3;
			index++;

			// Bottom left.
			cubeIndicesMaterial[index] = index4;
			index++;


			// Upper Right.
			cubeIndicesMaterial[index] = index1;
			index++;

			// Bottom right.
			cubeIndicesMaterial[index] = index4;
			index++;

			// Bottom left.
			cubeIndicesMaterial[index] = index2;
			index++;
		}
	}

	index = 0;
	// Load Mesh
	for (int j = 0; j < (gridHeight - 1); j++)
	{
		for (int i = 0; i < (gridWidth - 1); i++)
		{
			int	index1 = (gridHeight * j) + i;          // Bottom left.
			int index2 = (gridHeight * j) + (i + 1);      // Bottom right.
			int index3 = (gridHeight * (j + 1)) + i;      // Upper left.
			int index4 = (gridHeight * (j + 1)) + (i + 1);  // Upper right.

			// Upper left.
			cubeIndicesMesh[index] = index3;
			index++;

			// Upper right.
			cubeIndicesMesh[index] = index4;
			index++;

			// Upper right.
			cubeIndicesMesh[index] = index4;
			index++;

			// Bottom left
			cubeIndicesMesh[index] = index1;
			index++;

			// Bottom left.
			cubeIndicesMesh[index] = index1;
			index++;

			// Upper left.
			cubeIndicesMesh[index] = index3;
			index++;

			// Bottom left.
			cubeIndicesMesh[index] = index1;
			index++;

			// Upper right.
			cubeIndicesMesh[index] = index4;
			index++;

			// Upper right.
			cubeIndicesMesh[index] = index4;
			index++;

			// Bottom right.
			cubeIndicesMesh[index] = index2;
			index++;

			// Bottom right.
			cubeIndicesMesh[index] = index2;
			index++;

			// Bottom left.
			cubeIndicesMesh[index] = index1;
			index++;
		}
	}

	//index = 0;
	for (int j = 0; j < (gridHeight - 1); j++)
	{
		for (int i = 0; i < (gridWidth - 1); i++)
		{
			if (j != 0 && j != gridHeight - 2 && i != 0 && i != gridWidth - 2)
				continue;
			int index1, index2, index3, index4;
			if (j == 0)
			{
				//int index1 = (gridHeight * j) + i;          // Bottom left.
				//int index2 = (gridHeight * j) + (i + 1);      // Bottom right.
				//int index3 = (gridHeight * (j + 1)) + i;      // Upper left.
				//int index4 = (gridHeight * (j + 1)) + (i + 1);  // Upper right.

				index1 = (gridHeight * j) + i;          // Bottom left.
				index2 = (gridHeight * j) + (i + 1);      // Bottom right.
				index3 = index1 + gridHeight * gridWidth;      // Upper left.
				index4 = index2 + gridHeight * gridWidth;   // Upper right.
				fillIndices(index, index1, index2, index3, index4);
			}
			if (j == gridHeight - 2)
			{
				index3 = (gridHeight * (j + 1)) + i;      // Upper left.
				index4 = (gridHeight * (j + 1)) + (i + 1);  // Upper right.
				index1 = index3 + gridHeight * gridWidth;          // Bottom left.
				index2 = index4 + gridHeight * gridWidth;      // Bottom right.
				fillIndices(index, index1, index2, index3, index4);
			}
			if (i == 0)
			{
				index3 = (gridHeight * (j + 1)) + i;      // Upper left.
				index4 = (gridHeight * j) + i; //Upper right
				index1 = index3 + gridHeight * gridWidth; //Bottom left
				index2 = index4 + gridHeight * gridWidth; //Bottom right
				fillIndices(index, index1, index2, index3, index4);
			}
			if (i == gridWidth - 2)
			{
				index4 = (gridHeight * (j + 1)) + i + 1;    // Upper right.
				index3 = (gridHeight * j) + i + 1; //Upper left
				index1 = index3 + gridHeight * gridWidth; //Bottom left
				index2 = index4 + gridHeight * gridWidth; //Bottom right.
				fillIndices(index, index1, index2, index3, index4);
			}

		}
	}


	//int dataCount = ARRAYSIZE(triangleVertices);

	m_Vertexes = m_service.Device.CreateVertexBuffer(cubeVertices, verticesCount);
	indicesPointerMesh = m_service.Device.CreateIndexBuffer(cubeIndicesMesh, indicesMeshCount);
	indicesPointerMaterial = m_service.Device.CreateIndexBuffer(cubeIndicesMaterial, indicesMaterialCount);
	delete[] cubeIndicesMesh;
	//delete[] cubeIndicesMaterial;
}

void HeightMap::Draw()
{
	m_service.Context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
	//const XMMATRIX worldMtx = XMMatrixIdentity();
	//m_cbWorld->Update(m_context, worldMtx);

	ID3D11Buffer* b = m_Vertexes.get();
	m_service.Context->IASetVertexBuffers(0, 1, &b, &VB_STRIDE, &VB_OFFSET);
	m_service.Context->IASetIndexBuffer(indicesPointerMesh.get(), DXGI_FORMAT_R16_UINT, 0);
	m_service.Context->DrawIndexed(indicesMeshCount, 0, 0);

	if (shadingEnabled)
	{
		m_service.Context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
		m_service.Context->IASetIndexBuffer(indicesPointerMaterial.get(), DXGI_FORMAT_R16_UINT, 0);
		m_service.Context->DrawIndexed(indicesMaterialCount, 0, 0);
	}
}


VertexPosNormal& HeightMap::GetHeightMapValue(int x, int y)
{
	int index = gridWidth * x + y;
	return cubeVertices[index];
}

void HeightMap::fillIndices(int &index, int index1, int index2, int index3, int index4)
{

	// Upper left.
	cubeIndicesMesh[index] = index3;
	index++;

	// Upper right.
	cubeIndicesMesh[index] = index4;
	index++;

	// Upper right.
	cubeIndicesMesh[index] = index4;
	index++;

	// Bottom left
	cubeIndicesMesh[index] = index1;
	index++;

	// Bottom left.
	cubeIndicesMesh[index] = index1;
	index++;

	// Upper left.
	cubeIndicesMesh[index] = index3;
	index++;

	// Bottom left.
	cubeIndicesMesh[index] = index1;
	index++;

	// Upper right.
	cubeIndicesMesh[index] = index4;
	index++;

	// Upper right.
	cubeIndicesMesh[index] = index4;
	index++;

	// Bottom right.
	cubeIndicesMesh[index] = index2;
	index++;

	// Bottom right.
	cubeIndicesMesh[index] = index2;
	index++;

	// Bottom left.
	cubeIndicesMesh[index] = index1;
	index++;
}

void HeightMap::Update()
{
	m_Vertexes.reset();
	m_Vertexes = m_service.Device.CreateVertexBuffer(cubeVertices, verticesCount);
}