#pragma once

#include <d3d11.h>
#include <d3dx10math.h>
#include "modelclass.h"
//#include "gk2_vertices.h"


using namespace std;

namespace gk2{
	class HeightMap
	{
	public:
		HeightMap(Service& service, int _gridWidth = 150, int _gridHeight = 150, int _width = 150, int _height = 150, int minY = 20);
		HeightMap(const HeightMap&);
		~HeightMap();
		VertexPosNormal& GetHeightMapValue(int x, int y);
		void Draw();
		void Update();
		VertexPosNormal* cubeVertices;
		int gridWidth, gridHeight;
		int width, height;
		float minY;
		float scale = 0.01;
		bool shadingEnabled = false;
	private:
		static const unsigned int VB_STRIDE;
		static const unsigned int VB_OFFSET;

		Service m_service;
		XMFLOAT3* m_heightMap;
		XMFLOAT3* m_lowMap;

		void Initialize();
		void InitializeMap(int n, int m);
		void fillIndices(int &index, int index1, int index2, int index3, int index4);

		VertexPosNormal* lowVertices;
		int verticesCount;
		unsigned short* cubeIndicesMesh;
		unsigned short* cubeIndicesMaterial;
		std::shared_ptr<ID3D11Buffer> rimIndices;
		shared_ptr<ID3D11Buffer> indicesPointerMesh;
		shared_ptr<ID3D11Buffer> indicesPointerMaterial;
		std::shared_ptr<ID3D11Buffer> m_Vertexes;
		std::shared_ptr<ID3D11Buffer> m_Rim;

		int indicesRimCount;
		int indicesMeshCount;
		int indicesMaterialCount;
	};
}