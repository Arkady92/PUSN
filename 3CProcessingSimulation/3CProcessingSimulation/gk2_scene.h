#ifndef __GK2_SCENE_H_
#define __GK2_SCENE_H_

#include "gk2_applicationBase.h"
#include "gk2_camera.h"
#include <AntTweakBar.h>
#include <xnamath.h>
#include "coordinateSystem.h"
#include "miller.h"
#include "service.h"
#include "heightMap.h"

namespace gk2
{
	class Scene : public gk2::ApplicationBase
	{
	public:
		Scene(HINSTANCE hInstance);
		virtual ~Scene();

		static void* operator new(size_t size);
		static void operator delete(void* ptr);
		void LoadPaths(std::wstring fileName);
		void RecreateScene();
		void RecreateMiller();
		void ClearMaterial();
		void SetMap();

	protected:
		virtual bool LoadContent();
		virtual void UnloadContent();

		virtual void Update(float dt);
		virtual void Render();
	private:
		//Various D3D constants
		static const unsigned int VB_STRIDE;
		static const unsigned int VB_OFFSET;
		static const unsigned int BS_MASK;

		//Projection matrix
		XMMATRIX m_projMtx;

		//Camera helper
		gk2::Camera m_camera;
		Service service;

		//Shaders
		std::shared_ptr<ID3D11VertexShader> m_vertexShader;
		std::shared_ptr<ID3D11PixelShader> m_pixelShader;
		
		//VertexPosNormal input layout
		std::shared_ptr<ID3D11InputLayout> m_inputLayout;
		std::shared_ptr<ID3D11InputLayout> m_ilBilboard;


		HeightMap* m_HeightMap;

		CoordinateSystem* m_coordinateSystem;
		Miller* m_miller;

		bool modelAnimate = false;
		bool jumpToEnd = false;
		float modelAnimationSpeed = 0.5f;
		static float backgroundColor[4];
		static float lightDirection[3];
		XMFLOAT4 lightPositions[2];

		//Shader's constant buffer containing Local -> World matrix
		std::shared_ptr<ID3D11Buffer> m_cbWorld;
		//Shader's constant buffer containing World -> Camera matrix
		std::shared_ptr<ID3D11Buffer> m_cbView;
		//Shader's constant buffer containing projection matrix
		std::shared_ptr<ID3D11Buffer> m_cbProj;
		//Shader's constant buffer containing light's positions
		std::shared_ptr<ID3D11Buffer> m_cbLights;
		//Shader's constant buffer containting surface color
		std::shared_ptr<ID3D11Buffer> m_cbSurfaceColor;

		//Path to the shaders' file
		static const std::wstring ShaderFile;

		//Initializes shaders
		void InitializeShaders();
		//Initializes constant buffers
		void InitializeConstantBuffers();
		//Initializes all render states
		void InitializeRenderStates();
		//Initializes camera
		void InitializeCamera();

		//Handles keyboard actions
		bool CheckKeys(float dt);

		//Updates camera-related constant buffers
		void UpdateCamera(const XMMATRIX& view);

		//Binds shaders to the device context
		void SetShaders();
		//Binds constant buffers to shaders
		void SetConstantBuffers();
		//Binds bilboard shaders to the device context
		
		//Sets the value of the surface color constant buffer
		void SetSurfaceColor(const XMFLOAT4& color);

		//Sets up one white positional light at the camera position
		void SetLight();

		void CreateMap();
		void UpdateMap(float dt);
		void DrawMap();

		void InitializeService();
		void CreateCoordinateSystem();
		void DrawCoordinateSystem();
		void CreateMiller();
		void DrawMiller();
		void UpdateMiller(int *dx, int *dy, float step = 0.01);

		void CreateTweakBar();

		wstring fileName;
		int millerSize = 16;
		enum millerType millerType;
		std::vector<VertexPos> paths;
		int actualPath;

		int gridWidth = 100, gridHeight = 100;
		int width = 150, height = 150;
		float minY = 20;
		float lastFlatY = 0;
		const float EPSILON = 0.001;
		bool mapModificated = false;
		bool shadingEnabled = false;
		float minerr = 1000;
		float maxerr = 0;

		int Signum(float x);
		void SetMillerStartPosition();
		void ProcessFullSimulation();
		std::vector<XMFLOAT2> bresenhamPoints;
		std::vector<XMFLOAT2> bresenhamNewPoints;
		XMFLOAT3 CalculateNormal(int i, int j);
		void CalculateNormals();

	};

	enum millerType
	{
		K16,
		K08,
		K01,
		F12,
		F10
	};
}

#endif __GK2_SCENE_H_