#ifndef __GK2_SCENE_H_
#define __GK2_SCENE_H_

#include "gk2_applicationBase.h"
#include "gk2_camera.h"
#include <AntTweakBar.h>
#include <xnamath.h>

namespace gk2
{
	class Scene : public gk2::ApplicationBase
	{
	public:
		Scene(HINSTANCE hInstance);
		virtual ~Scene();

		static void* operator new(size_t size);
		static void operator delete(void* ptr);

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

		//Shaders
		std::shared_ptr<ID3D11VertexShader> m_vertexShader;
		std::shared_ptr<ID3D11PixelShader> m_pixelShader;
		
		//VertexPosNormal input layout
		std::shared_ptr<ID3D11InputLayout> m_inputLayout;
		std::shared_ptr<ID3D11InputLayout> m_ilBilboard;

		//Model vertex buffer
		std::shared_ptr<ID3D11Buffer> m_vbModel;
		//Model index buffer
		std::shared_ptr<ID3D11Buffer> m_ibModel;
		//Model indices count
		int m_ibModelCount = 0;
		//Model transformation matrix
		XMMATRIX m_ModelMtx;

		bool modelAnimate = true;
		float modelAnimationSpeed = 0.5f;
		static float backgroundColor[4];
		static float lightDirection[3];

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
		bool CheckKeys();

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

		void CreateModel();
		void UpdateModel(float dt);
		void DrawModel();

		void CreateTweakBar();

	};
}

#endif __GK2_SCENE_H_