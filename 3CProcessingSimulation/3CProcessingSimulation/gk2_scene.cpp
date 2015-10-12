#include "gk2_scene.h"
#include "gk2_utils.h"
#include "gk2_vertices.h"
#include "gk2_window.h"

using namespace std;
using namespace gk2;

#define RESOURCES_PATH L"resources/"
const wstring Scene::ShaderFile = RESOURCES_PATH L"shaders/shader.hlsl";

const unsigned int Scene::VB_STRIDE = sizeof(VertexPosNormal);
const unsigned int Scene::VB_OFFSET = 0;

void* Scene::operator new(size_t size)
{
	return Utils::New16Aligned(size);
}

void Scene::operator delete(void* ptr)
{
	Utils::Delete16Aligned(ptr);
}

Scene::Scene(HINSTANCE hInstance)
	: ApplicationBase(hInstance), m_camera(0.0f, 50.0f)
{

}

Scene::~Scene()
{

}

void Scene::InitializeShaders()
{
	shared_ptr<ID3DBlob> vsByteCode = m_device.CompileD3DShader(ShaderFile, "VS_Main", "vs_4_0");
	shared_ptr<ID3DBlob> psByteCode = m_device.CompileD3DShader(ShaderFile, "PS_Main", "ps_4_0");
	m_vertexShader = m_device.CreateVertexShader(vsByteCode);
	m_pixelShader = m_device.CreatePixelShader(psByteCode);
	m_inputLayout = m_device.CreateInputLayout<VertexPosNormal>(vsByteCode);
}

void Scene::InitializeConstantBuffers()
{
	D3D11_BUFFER_DESC desc;
	ZeroMemory(&desc, sizeof(D3D11_BUFFER_DESC));
	desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	desc.ByteWidth = sizeof(XMMATRIX);
	desc.Usage = D3D11_USAGE_DEFAULT;
	m_cbWorld = m_device.CreateBuffer(desc);
	m_cbProj = m_device.CreateBuffer(desc);
	desc.ByteWidth = sizeof(XMMATRIX) * 2;
	m_cbView = m_device.CreateBuffer(desc);
	desc.ByteWidth = sizeof(XMFLOAT4) * 3;
	m_cbLights = m_device.CreateBuffer(desc);
	desc.ByteWidth = sizeof(XMFLOAT4);
	m_cbSurfaceColor = m_device.CreateBuffer(desc);
}

void Scene::InitializeRenderStates()
{
}

void Scene::InitializeCamera()
{
	SIZE s = getMainWindow()->getClientSize();
	float ar = static_cast<float>(s.cx) / s.cy;
	m_projMtx = XMMatrixPerspectiveFovLH(XM_PIDIV4, ar, 0.01f, 100.0f);
	m_context->UpdateSubresource(m_cbProj.get(), 0, 0, &m_projMtx, 0, 0);
	m_camera.Zoom(2.5);
	UpdateCamera(m_camera.GetViewMatrix());
}

void Scene::SetShaders()
{
	m_context->VSSetShader(m_vertexShader.get(), 0, 0);
	m_context->PSSetShader(m_pixelShader.get(), 0, 0);
	m_context->IASetInputLayout(m_inputLayout.get());
	m_context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
}

void Scene::SetConstantBuffers()
{
	ID3D11Buffer* vsb[] = { m_cbWorld.get(), m_cbView.get(), m_cbProj.get(), m_cbLights.get() };
	m_context->VSSetConstantBuffers(0, 4, vsb);
	ID3D11Buffer* psb[] = { m_cbSurfaceColor.get() };
	m_context->PSSetConstantBuffers(0, 1, psb);
}

bool Scene::LoadContent()
{
	InitializeShaders();
	InitializeConstantBuffers();
	InitializeRenderStates();
	InitializeCamera();
	SetShaders();
	SetConstantBuffers();
	CreateModel();
	CreateTweakBar();
	return true;
}

void Scene::UnloadContent()
{
	m_vertexShader.reset();
	m_pixelShader.reset();
	m_inputLayout.reset();

	m_vbModel.reset();
	m_ibModel.reset();

	m_cbWorld.reset();
	m_cbView.reset();
	m_cbProj.reset();
	m_cbLights.reset();
	m_cbSurfaceColor.reset();
}

void Scene::UpdateCamera(const XMMATRIX& view)
{
	XMMATRIX viewMtx[2];
	viewMtx[0] = view;
	XMVECTOR det;
	viewMtx[1] = XMMatrixInverse(&det, viewMtx[0]);
	m_context->UpdateSubresource(m_cbView.get(), 0, 0, viewMtx, 0, 0);
}

void Scene::SetSurfaceColor(const XMFLOAT4& color)
{
	m_context->UpdateSubresource(m_cbSurfaceColor.get(), 0, 0, &color, 0, 0);
}

void Scene::SetLight()
{
	m_context->UpdateSubresource(m_cbLights.get(), 0, 0, lightDirection, 0, 0);
}

bool Scene::CheckKeys()
{
	bool change = false;
	const float step = 1 / 300.f;
	KeyboardState currentKeyboardState;
	if (m_keyboard->GetState(currentKeyboardState))
	{

		if (currentKeyboardState.isKeyDown(DIK_W) || currentKeyboardState.isKeyDown(DIK_UP))
		{
			m_camera.MoveCamera(0, step);
			change = true;
		}

		if (currentKeyboardState.isKeyDown(DIK_S) || currentKeyboardState.isKeyDown(DIK_DOWN))
		{
			m_camera.MoveCamera(0, -step);
			change = true;
		}

		if (currentKeyboardState.isKeyDown(DIK_A) || currentKeyboardState.isKeyDown(DIK_LEFT))
		{
			m_camera.MoveCamera(-step, 0);
			change = true;
		}

		if (currentKeyboardState.isKeyDown(DIK_D) || currentKeyboardState.isKeyDown(DIK_RIGHT))
		{
			m_camera.MoveCamera(step, 0);
			change = true;
		}
	}
	return change;
}

void Scene::Update(float dt)
{
	static MouseState prevState;
	MouseState currentState;
	if (!m_mouse->GetState(currentState))
		return;
	bool change = true;
	if (prevState.isButtonDown(0))
	{
		POINT d = currentState.getMousePositionChange();
		m_camera.Rotate(d.y / 300.f, d.x / 300.f);
	}
	else if (prevState.isButtonDown(1))
	{
		POINT d = currentState.getMousePositionChange();
		m_camera.Zoom(d.y / 50.0f);
	}
	else
		change = false;
	if (CheckKeys())
		change = true;
	prevState = currentState;
	if (change)
		UpdateCamera(m_camera.GetViewMatrix());
	if (modelAnimate)
		UpdateModel(dt);
}

void Scene::CreateModel()
{
	unsigned short indices[] =
	{
		0, 2, 1, 0, 3, 2,
		4, 6, 5, 4, 7, 6,
		8, 10, 9, 8, 11, 10,
		12, 14, 13, 12, 15, 14,
		16, 18, 17, 16, 19, 18,
		20, 22, 21, 20, 23, 22
	};

	float size = 1.0f;

	VertexPosNormal vertices[] =
	{
		{ XMFLOAT3(-size / 2, -size / 2, -size / 2), XMFLOAT3(0.0f, 0.0f, -1.0f) },
		{ XMFLOAT3(size / 2, -size / 2, -size / 2), XMFLOAT3(0.0f, 0.0f, -1.0f) },
		{ XMFLOAT3(size / 2, size / 2, -size / 2), XMFLOAT3(0.0f, 0.0f, -1.0f) },
		{ XMFLOAT3(-size / 2, size / 2, -size / 2), XMFLOAT3(0.0f, 0.0f, -1.0f) },

		{ XMFLOAT3(-size / 2, -size / 2, -size / 2), XMFLOAT3(-1.0f, 0.0f, 0.0f) },
		{ XMFLOAT3(-size / 2, size / 2, -size / 2), XMFLOAT3(-1.0f, 0.0f, 0.0f) },
		{ XMFLOAT3(-size / 2, size / 2, size / 2), XMFLOAT3(-1.0f, 0.0f, 0.0f) },
		{ XMFLOAT3(-size / 2, -size / 2, size / 2), XMFLOAT3(-1.0f, 0.0f, 0.0f) },

		{ XMFLOAT3(-size / 2, -size / 2, -size / 2), XMFLOAT3(0.0f, -1.0f, 0.0f) },
		{ XMFLOAT3(-size / 2, -size / 2, size / 2), XMFLOAT3(0.0f, -1.0f, 0.0f) },
		{ XMFLOAT3(size / 2, -size / 2, size / 2), XMFLOAT3(0.0f, -1.0f, 0.0f) },
		{ XMFLOAT3(size / 2, -size / 2, -size / 2), XMFLOAT3(0.0f, -1.0f, 0.0f) },

		{ XMFLOAT3(-size / 2, -size / 2, size / 2), XMFLOAT3(0.0f, 0.0f, 1.0f) },
		{ XMFLOAT3(-size / 2, size / 2, size / 2), XMFLOAT3(0.0f, 0.0f, 1.0f) },
		{ XMFLOAT3(size / 2, size / 2, size / 2), XMFLOAT3(0.0f, 0.0f, 1.0f) },
		{ XMFLOAT3(size / 2, -size / 2, size / 2), XMFLOAT3(0.0f, 0.0f, 1.0f) },

		{ XMFLOAT3(size / 2, -size / 2, -size / 2), XMFLOAT3(1.0f, 0.0f, 0.0f) },
		{ XMFLOAT3(size / 2, -size / 2, size / 2), XMFLOAT3(1.0f, 0.0f, 0.0f) },
		{ XMFLOAT3(size / 2, size / 2, size / 2), XMFLOAT3(1.0f, 0.0f, 0.0f) },
		{ XMFLOAT3(size / 2, size / 2, -size / 2), XMFLOAT3(1.0f, 0.0f, 0.0f) },

		{ XMFLOAT3(-size / 2, size / 2, -size / 2), XMFLOAT3(0.0f, 1.0f, 0.0f) },
		{ XMFLOAT3(size / 2, size / 2, -size / 2), XMFLOAT3(0.0f, 1.0f, 0.0f) },
		{ XMFLOAT3(size / 2, size / 2, size / 2), XMFLOAT3(0.0f, 1.0f, 0.0f) },
		{ XMFLOAT3(-size / 2, size / 2, size / 2), XMFLOAT3(0.0f, 1.0f, 0.0f) },
	};

	m_vbModel = m_device.CreateVertexBuffer(vertices, 24);
	m_ibModel = m_device.CreateIndexBuffer(indices, 36);
	m_ibModelCount = 36;
	m_ModelMtx = XMMatrixIdentity();
}

void Scene::DrawModel()
{
	SetSurfaceColor(XMFLOAT4(0.35f, 0.35f, 0.45f, 1.0f));
	ID3D11Buffer* b = m_vbModel.get();
	m_context->IASetVertexBuffers(0, 1, &b, &VB_STRIDE, &VB_OFFSET);
	m_context->IASetIndexBuffer(m_ibModel.get(), DXGI_FORMAT_R16_UINT, 0);
	m_context->UpdateSubresource(m_cbWorld.get(), 0, 0, &m_ModelMtx, 0, 0);
	m_context->DrawIndexed(m_ibModelCount, 0, 0);
}

void Scene::UpdateModel(float dt)
{
	float angle = modelAnimationSpeed * dt;
	m_ModelMtx = m_ModelMtx * XMMatrixRotationX(angle) * XMMatrixRotationY(angle) * XMMatrixRotationZ(angle);
}

void Scene::Render()
{
	if (m_context == nullptr)
		return;

	//Clear buffers
	m_context->ClearRenderTargetView(m_backBuffer.get(), backgroundColor);
	m_context->ClearDepthStencilView(m_depthStencilView.get(), D3D11_CLEAR_DEPTH | D3D11_CLEAR_STENCIL, 1.0f, 0);

	// Render scene
	SetLight();
	DrawModel();
	TwDraw();

	m_swapChain->Present(0, 0);
}

void TW_CALL LoadFileCallback(void * a) 
{
	Window* mainWindow = static_cast<Window*>(a); 
	OPENFILENAME ofn = { 0 }; 
	TCHAR szFilters[] = TEXT("Instructions files (*.f??)\0*.f*\0Instructions files (*.k??)\0*.k*\0\0"); //txt files (*.txt)|*.txt|All files (*.*)|*.*"
	TCHAR szFilePathName[_MAX_PATH] = TEXT(""); 
	ofn.lStructSize = sizeof(OPENFILENAME); 
	ofn.hwndOwner = mainWindow->getHandle();
	ofn.lpstrFilter = szFilters; 
	ofn.lpstrFile = szFilePathName; 
	ofn.nMaxFile = _MAX_PATH; 
	ofn.lpstrTitle = TEXT("Open File"); 
	ofn.Flags = OFN_FILEMUSTEXIST; 
	GetOpenFileName(&ofn); 
	wstring s = (wstring)(ofn.lpstrFile);
}

float Scene::backgroundColor[4] = { 0, 0, 0, 1 };
float Scene::lightDirection[3] = { -0.5f, -0.2f, 1 };

void Scene::CreateTweakBar()
{
	// Create a tweak bar
	TwBar *bar = TwNewBar("Options");
	TwDefine(" GLOBAL help='.' Free C3C Processing Simulator."); // Message added to the help bar.
	int barSize[2] = { 224, 320 };
	TwSetParam(bar, NULL, "size", TW_PARAM_INT32, 2, barSize);

	// Add variables to the tweak bar
	TwAddButton(bar, "LoadFile", LoadFileCallback, getMainWindow(), "label='Load File'");
	TwAddVarRW(bar, "Animation", TW_TYPE_BOOLCPP, &modelAnimate, "group=Sponge key=a");
	TwAddVarRW(bar, "Animation speed", TW_TYPE_FLOAT, &modelAnimationSpeed, "min=-10 max=10 step=0.1 group=Sponge keyincr=+ keydecr=-");
	TwAddVarRW(bar, "Light direction", TW_TYPE_DIR3F, &lightDirection, "opened=true axisz=-z showval=false");
	TwAddVarRW(bar, "Background", TW_TYPE_COLOR4F, &backgroundColor, "colormode=hls");
}