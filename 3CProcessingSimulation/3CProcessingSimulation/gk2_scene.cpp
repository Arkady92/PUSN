#include "gk2_scene.h"
#include "gk2_utils.h"
#include "gk2_vertices.h"
#include "gk2_window.h"
#include <fstream>
#include <iostream>

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
	m_camera.Zoom(5);
	m_camera.Rotate(XM_PIDIV4, 0.0f);
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
	InitializeService();
	CreateCoordinateSystem();
	CreateMiller();
	CreateMap();
	CreateTweakBar();
	return true;
}

void Scene::UnloadContent()
{
	m_vertexShader.reset();
	m_pixelShader.reset();
	m_inputLayout.reset();

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
	lightPositions[0] = XMFLOAT4(-lightDirection[0], -lightDirection[1], -lightDirection[1], 1);
	lightPositions[1] = XMFLOAT4(lightDirection[0], lightDirection[1], lightDirection[1], 1);
	m_context->UpdateSubresource(m_cbLights.get(), 0, 0, lightPositions, 0, 0);
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
	if (modelAnimate && paths.size() > 0 && !jumpToEnd)
	{
		UpdateMap(dt);
		UpdateMiller(NULL, NULL);
	}
	if (jumpToEnd)
	{
		modelAnimate = true;
		ProcessFullSimulation();
		jumpToEnd = false;
	}
}

void Scene::InitializeService()
{
	service.Context = m_context;
	service.Device = m_device;
	service.cbSurfaceColor = m_cbSurfaceColor;
	service.Scene = this;
}

void Scene::CreateCoordinateSystem()
{
	m_coordinateSystem = new CoordinateSystem(service);
}

void Scene::CreateMiller()
{
	m_miller = new Miller(service);
}

void Scene::CreateMap()
{
	m_HeightMap = new HeightMap(service);
	for (size_t i = 0; i < 200; i++)
	{
		for (size_t j = 0; j < 200; j++)
		{
			activityMap[i][j] = false;
		}
	}
}

void Scene::RecreateMiller()
{
	char type;
	switch (millerType)
	{
	case gk2::K16:
		type = 'k';
		break;
	case gk2::K08:
		type = 'k';
		break;
	case gk2::K01:
		type = 'k';
		break;
	case gk2::F12:
		type = 'f';
		break;
	case gk2::F10:
		type = 'f';
		break;
	default:
		break;
	}

	delete m_miller;
	m_miller = new Miller(service, millerSize, type);
	SetMillerStartPosition();
}

void Scene::RecreateScene()
{
	delete m_HeightMap;
	delete m_miller;
	char type;
	switch (millerType)
	{
	case gk2::K16:
		type = 'k';
		millerSize = 16;
		break;
	case gk2::K08:
		type = 'k';
		millerSize = 8;
		break;
	case gk2::K01:
		type = 'k';
		millerSize = 1;
		break;
	case gk2::F12:
		type = 'f';
		millerSize = 12;
		break;
	case gk2::F10:
		type = 'f';
		millerSize = 10;
		break;
	default:
		break;
	}
	m_miller = new Miller(service, millerSize, type);
	m_HeightMap = new HeightMap(service, gridWidth, gridHeight, width, height, minY);
	SetMillerStartPosition();
}

void Scene::ClearMaterial()
{
	delete m_HeightMap;
	m_HeightMap = new HeightMap(service, gridWidth, gridHeight, width, height, minY);
}

void Scene::DrawCoordinateSystem()
{
	m_coordinateSystem->Draw();
}

int Scene::Signum(float x)
{
	return (x > 0) ? 1 : (x < 0) ? -1 : 0;
}

void Scene::DrawMiller()
{
	XMMATRIX worldMtx;
	m_miller->GetModelMatrix(worldMtx);
	m_context->UpdateSubresource(m_cbWorld.get(), 0, 0, &worldMtx, 0, 0);
	m_miller->Draw();
	m_context->UpdateSubresource(m_cbWorld.get(), 0, 0, &XMMatrixIdentity(), 0, 0);
}

void Scene::DrawMap()
{
	m_HeightMap->Draw();
}

void Scene::SetMillerStartPosition()
{
	if (paths.size() > 0)
	{
		auto pos = m_miller->GetPosition();
		m_miller->Translate(XMFLOAT4(paths[0].Pos.x - pos.x, paths[0].Pos.y - pos.y + m_miller->millerSize / 200.0f, paths[0].Pos.z - pos.z, 1));
		actualPath = 1;
	}
}

void Scene::UpdateMiller(int *dx, int * dy, float step)
{
	auto pos = m_miller->GetPosition();
	auto nextPos = paths[actualPath].Pos;
	nextPos.y += (m_miller->millerSize / 200.0f);
	if (abs(pos.x - nextPos.x) + abs(pos.y - nextPos.y) + abs(pos.z - nextPos.z) < 0.01)
	{
		actualPath++;
		if (actualPath >= paths.size())
		{
			modelAnimate = false;
			return;
		}
		nextPos = paths[actualPath].Pos;
	}
	float shift = 0.002;
	m_miller->Translate(XMFLOAT4(shift * Signum(nextPos.x - pos.x), shift * Signum(nextPos.y - pos.y), shift * Signum(nextPos.z - pos.z), 1));
	if (dx != NULL)
		*dx = Signum(nextPos.x - pos.x);
	if (dy != NULL)
		*dy = Signum(nextPos.y - pos.y);
}

void Scene::UpdateMap(float dt)
{
	for (int i = 0; i < m_HeightMap->gridHeight * m_HeightMap->gridWidth; i++)
	{
		XMFLOAT3 p = m_HeightMap->cubeVertices[i].Pos;
		XMVECTOR pos = XMVectorSet(p.x, p.y, p.z, 1);
		if (m_miller->CheckIfPointIsInside(pos))
		{
			if (XMVectorGetY(pos) < m_HeightMap->minY / 100.0f)
			{
				MessageBoxW(NULL, L"You've crossed the line", L"Error", MB_OK);
				modelAnimate = false;
				return;
			}
			if (m_miller->millerType == 'f' && m_HeightMap->cubeVertices[i].Pos.y < XMVectorGetY(pos))
			{
				MessageBoxW(NULL, L"You've destroyed the miller", L"Error", MB_OK);
				modelAnimate = false;
				return;
			}
			m_HeightMap->cubeVertices[i].Pos.y = XMVectorGetY(pos);
		}
	}
	m_HeightMap->Update();
}

void Scene::ProcessFullSimulation()
{
	bresenhamPoints.clear();
	while (bresenhamPoints.size() == 0)
	{
		for (int i = 0; i < m_HeightMap->gridWidth; i++)
		{
			for (int j = 0; j < m_HeightMap->gridHeight; j++)
			{
				XMFLOAT3 p = m_HeightMap->cubeVertices[i * m_HeightMap->gridHeight + j].Pos;
				XMVECTOR pos = XMVectorSet(p.x, p.y, p.z, 1);
				if (m_miller->CheckIfPointIsInside(pos))
				{
					m_HeightMap->cubeVertices[i].Pos.y = XMVectorGetY(pos);
					bresenhamPoints.push_back(XMFLOAT2(i, j));
					activityMap[i][j] = true;
				}
			}
		}
		if (bresenhamPoints.size() == 0)
			UpdateMiller(NULL, NULL);
	}
	float step = 1.0 / m_HeightMap->gridWidth;
	while (modelAnimate && bresenhamPoints.size() != 0)
	{
		int dx, dy;
		UpdateMiller(&dx, &dy, step);
		for (vector<XMFLOAT2>::iterator it = bresenhamPoints.begin(); it != bresenhamPoints.end(); ++it)
		{
			int i = (*it).x;
			int j = (*it).y;
			activityMap[i][j] = false;
			XMFLOAT3 p;
			XMVECTOR pos;
			p = m_HeightMap->cubeVertices[(i)* m_HeightMap->gridHeight + j].Pos;
			pos = XMVectorSet(p.x, p.y, p.z, 1);
			if (m_miller->CheckIfPointIsInside(pos))
			{
				m_HeightMap->cubeVertices[i].Pos.y = XMVectorGetY(pos);
				if (!activityMap[i][j])
				{
					bresenhamNewPoints.push_back(XMFLOAT2(i, j));
					activityMap[i][j] = true;
				}
			}
			if (dy != 0)
			{
				p = m_HeightMap->cubeVertices[(i)* m_HeightMap->gridHeight + j + dy].Pos;
				pos = XMVectorSet(p.x, p.y, p.z, 1);
				if (m_miller->CheckIfPointIsInside(pos))
				{
					m_HeightMap->cubeVertices[i].Pos.y = XMVectorGetY(pos);
					if (!activityMap[i][j + dy])
					{
						bresenhamNewPoints.push_back(XMFLOAT2(i, j + dy));
						activityMap[i][j + dy] = true;
					}
				}
			}
			if (dx != 0)
			{
				p = m_HeightMap->cubeVertices[(i + dx)* m_HeightMap->gridHeight + j].Pos;
				pos = XMVectorSet(p.x, p.y, p.z, 1);
				if (m_miller->CheckIfPointIsInside(pos))
				{
					m_HeightMap->cubeVertices[i].Pos.y = XMVectorGetY(pos);
					if (!activityMap[i + dx][j])
					{
						bresenhamNewPoints.push_back(XMFLOAT2(i + dx, j));
						activityMap[i + dx][j] = true;
					}
				}
			}
			if (dx != 0 && dy != 0)
			{
				p = m_HeightMap->cubeVertices[(i + dx)* m_HeightMap->gridHeight + j + dy].Pos;
				pos = XMVectorSet(p.x, p.y, p.z, 1);
				if (m_miller->CheckIfPointIsInside(pos))
				{
					m_HeightMap->cubeVertices[i].Pos.y = XMVectorGetY(pos);
					if (!activityMap[i + dx][j + dy])
					{
						bresenhamNewPoints.push_back(XMFLOAT2(i + dx, j + dy));
						activityMap[i + dx][j + dy] = true;
					}
				}
			}
		}
		bresenhamPoints.clear();
		for (vector<XMFLOAT2>::iterator it = bresenhamNewPoints.begin(); it != bresenhamNewPoints.end(); ++it)
		{
			bresenhamPoints.push_back(*it);
		}
		bresenhamNewPoints.clear();
	}
	modelAnimate = false;
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
	DrawCoordinateSystem();
	DrawMap();
	DrawMiller();

	TwDraw();

	m_swapChain->Present(0, 0);
}

void Scene::LoadPaths(wstring fileName)
{
	if (fileName.length() <= 0) return;
	paths.clear();
	int pos = 0;
	while (fileName[pos++] != '.');
	switch (fileName[pos++])
	{
	case 'k':
		millerSize = 10 * (fileName[pos++] - '0') + fileName[pos] - '0';
		switch (millerSize)
		{
		case 16:
			millerType = millerType::K16;
			break;
		case 8:
			millerType = millerType::K08;
			break;
		case 1:
			millerType = millerType::K01;
			break;
		default:
			break;
		}
		break;
	case 'f':
		millerSize = 10 * (fileName[pos++] - '0') + fileName[pos] - '0';
		switch (millerSize)
		{
		case 12:
			millerType = millerType::F12;
			break;
		case 10:
			millerType = millerType::F10;
			break;
		default:
			break;
		}
		break;
	default:
		break;
	}
	ifstream input;
	//input.exceptions(ios::badbit | ios::failbit);
	input.open(fileName);
	while (!input.eof())
	{
		VertexPos vs;
		input.get();
		input.get();
		if (input.eof()) break;
		while (input.get() != 'X');
		input >> vs.Pos.x;
		vs.Pos.x /= m_HeightMap->width;
		input.get();
		input >> vs.Pos.z;
		vs.Pos.z /= m_HeightMap->width;
		input.get();
		input >> vs.Pos.y;
		vs.Pos.y /= m_HeightMap->width;
		paths.push_back(vs);
	}
	input.close();
	RecreateMiller();
}

void TW_CALL LoadFileCallback(void * a)
{
	Service* service = static_cast<Service*>(a);
	Scene *scene = static_cast<Scene*>(service->Scene);
	OPENFILENAME ofn = { 0 };
	TCHAR szFilters[] = TEXT("Instructions files (*.f??)\0*.f*\0Instructions files (*.k??)\0*.k*\0\0"); //txt files (*.txt)|*.txt|All files (*.*)|*.*"
	TCHAR szFilePathName[_MAX_PATH] = TEXT("");
	ofn.lStructSize = sizeof(OPENFILENAME);
	ofn.hwndOwner = scene->getMainWindow()->getHandle();
	ofn.lpstrFilter = szFilters;
	ofn.lpstrFile = szFilePathName;
	ofn.nMaxFile = _MAX_PATH;
	ofn.lpstrTitle = TEXT("Open File");
	ofn.Flags = OFN_FILEMUSTEXIST;
	GetOpenFileName(&ofn);
	wstring s = (wstring)(ofn.lpstrFile);
	scene->LoadPaths(s);
}

void TW_CALL ApplyChangesCallback(void * a)
{
	Service* service = static_cast<Service*>(a);
	Scene *scene = static_cast<Scene*>(service->Scene);
	scene->RecreateScene();
}

void TW_CALL ClearMaterialCallback(void * a)
{
	Service* service = static_cast<Service*>(a);
	Scene *scene = static_cast<Scene*>(service->Scene);
	scene->ClearMaterial();
}

float Scene::backgroundColor[4] = { 0, 0, 0, 1 };
float Scene::lightDirection[3] = { 0.0f, 1.0f, 0.0f };

void Scene::CreateTweakBar()
{
	// Create a tweak bar
	TwBar *bar = TwNewBar("Options");
	TwDefine(" GLOBAL help='.' Free C3C Processing Simulator."); // Message added to the help bar.
	int barSize[2] = { 224, 350 };
	TwSetParam(bar, NULL, "size", TW_PARAM_INT32, 2, barSize);

	TwEnumVal twMillerTypeVal[] = { { millerType::K16, "K16" }, { millerType::K08, "K08" }, { millerType::K01, "K01" },
	{ millerType::F12, "F12" }, { millerType::F10, "F10" }, };
	TwType twMillerType = TwDefineEnum("MillerType", twMillerTypeVal, 5);

	// Add variables to the tweak bar
	TwAddButton(bar, "LoadFile", LoadFileCallback, &service, "label='Load File'");
	TwAddButton(bar, "ClearMaterial", ClearMaterialCallback, &service, "label='Clear Material'");
	TwAddVarRW(bar, "Play", TW_TYPE_BOOLCPP, &modelAnimate, "group=Animation");
	TwAddVarRW(bar, "To End", TW_TYPE_BOOLCPP, &jumpToEnd, "group=Animation");
	//TwAddVarRW(bar, "Animation speed", TW_TYPE_FLOAT, &modelAnimationSpeed, "min=-10 max=10 step=0.1 group=Animation keyincr=+ keydecr=-");
	TwAddVarRW(bar, "Miller Type", twMillerType, &millerType, "group=Parameters");
	TwAddVarRW(bar, "Grid Rows", TW_TYPE_INT32, &(gridWidth), "group=Parameters");
	TwAddVarRW(bar, "Grid Cols", TW_TYPE_INT32, &(gridHeight), "group=Parameters");
	TwAddVarRW(bar, "Width", TW_TYPE_INT32, &(width), "group=Parameters");
	TwAddVarRW(bar, "Hight", TW_TYPE_INT32, &(height), "group=Parameters");
	TwAddVarRW(bar, "Maximum Depth", TW_TYPE_FLOAT, &(minY), "group=Parameters");
	TwAddButton(bar, "Apply Changes", ApplyChangesCallback, &service, "label='Apply Changes'");


	TwAddVarRW(bar, "Light direction", TW_TYPE_DIR3F, &lightDirection, "opened=true axisz=-z showval=false");
	TwAddVarRW(bar, "Background", TW_TYPE_COLOR4F, &backgroundColor, "colormode=rgb");
}