#include "miller.h"

using namespace gk2;


Miller::Miller(Service &service, int _millerSize, char _millerType) : ModelClass(service)
{
	//m_Type = ModelType::CursorType;
	millerSize = _millerSize;
	millerType = _millerType;
	m_radius = millerSize / 200.0;
	ModelClass::Initialize();
	Miller::Initialize();
	if (millerType == 'k')
		Translate(XMFLOAT4(0, m_radius, 0, 1));
}


Miller::Miller(const Miller& other) : ModelClass(other)
{
}


Miller::~Miller()
{
	ModelClass::~ModelClass();
}

bool Miller::CheckIfPointIsInside(XMVECTOR& pos)
{
	XMVECTOR miller_center = XMVectorSet(GetPosition3().x, GetPosition3().y, GetPosition3().z, 1);
	if (XMVectorGetY(pos) > XMVectorGetY(miller_center))
		pos = XMVectorSetY(pos, XMVectorGetY(miller_center));
	XMVECTOR len = (pos - miller_center) * (pos - miller_center);
	float abs = 0;
	float l = XMVectorGetX(XMVector3Length(len));
	float diff = l - m_radius * m_radius;

	if (l < (m_radius * m_radius - abs))
	{
		if (millerType == 'k')
			calculateOffset(pos, miller_center);
		return true;
	}
	return false;
}

void Miller::calculateOffset(XMVECTOR& pos, XMVECTOR& miller_center)
{
	XMVECTOR o, cen, l, d;
	float r = m_radius;
	o = pos;
	cen = miller_center;
	l = XMVectorSet(0, -1, 0, 1);

	double A = XMVectorGetX(XMVector3LengthSq(l));
	XMVECTOR B_vec = 2 * (l * (o - cen));
	double B = XMVectorGetX(B_vec) + XMVectorGetY(B_vec) + XMVectorGetZ(B_vec);
	XMVECTOR C_vec = -2 * o * cen;
	float c_val = XMVectorGetX(C_vec) + XMVectorGetY(C_vec) + XMVectorGetZ(C_vec);
	double C = XMVectorGetX(XMVector3LengthSq(o)) + c_val + XMVectorGetX(XMVector3LengthSq(cen)) - m_radius * m_radius;;

	// discriminant
	double D = B * B - 4 * A * C;

	if (D < 0)
	{
		return;
	}

	double t1 = (-B - sqrt(D)) / (2.0 * A);
	double t2 = (-B + sqrt(D)) / (2.0 * A);
	double t = t1 > t2 ? t1 : t2;
	float oldVal = XMVectorGetY(pos);
	float val = XMVectorGetY(l) - XMVectorGetY(pos);
	float newVal = oldVal + val * t;
	if (newVal > oldVal)
		return;
	pos = XMVectorSetY(pos, newVal);

}


void Miller::Initialize()
{
	float height = m_radius;
	float radius = m_radius;
	float topRadius = 1.0f;
	float bottomRadius = 1.0f;
	int sliceCount = 30;
	int stackCount = 30;
	float stackHeight = height / (float)stackCount;
	float radiusStep = (topRadius - bottomRadius) / stackCount;
	int ringCount = stackCount + 1;

	if (millerType == 'k')
	{
		verticesCount = (stackCount - 1) * (sliceCount + 1) + 2;
		indicesCount = sliceCount * 3 + sliceCount * 3 + (stackCount - 2) * sliceCount * 6;
		VertexPosNormal* vertices = new VertexPosNormal[verticesCount];
		unsigned short* indices = new unsigned short[indicesCount];
		vertices[0].Pos = XMFLOAT3(0, radius, 0);
		vertices[0].Normal = XMFLOAT3(0, 0, 1);
		float phiStep = XM_PI / (float)stackCount;
		float thetaStep = XM_2PI / (float)sliceCount;

		for (int i = 1; i <= stackCount - 1; i++) {
			float phi = i*phiStep;
			for (int j = 0; j <= sliceCount; j++) {
				float theta = j*thetaStep;
				XMVECTOR p = XMVectorSet(
					(radius*sin(phi)*cos(theta)),
					(radius*cos(phi)),
					(radius*sin(phi)*sin(theta)),
					1
					);

				int index = (i - 1) * (sliceCount + 1) + j + 1;
				vertices[index].Pos = XMFLOAT3(XMVectorGetX(p), XMVectorGetY(p), XMVectorGetZ(p));
				vertices[index].Normal = XMFLOAT3(0, 0, 1);
			}
		}
		vertices[verticesCount - 1].Pos = XMFLOAT3(0, 0 - radius, 0);
		vertices[verticesCount - 1].Normal = XMFLOAT3(0, 0, 1);

		for (int i = 1; i <= sliceCount; i++) {
			int index = i * 3;
			indices[index++] = 0;
			indices[index++] = i + 1;
			indices[index++] = i;
		}
		int baseIndex = 1;
		int ringVertexCount = sliceCount + 1;
		for (int i = 0; i < stackCount - 2; i++) {
			for (int j = 0; j < sliceCount; j++) {
				int index = (i * sliceCount + j) * 6 + sliceCount * 3;
				indices[index++] = baseIndex + i*ringVertexCount + j;
				indices[index++] = baseIndex + i*ringVertexCount + j + 1;
				indices[index++] = baseIndex + (i + 1)*ringVertexCount + j;
				indices[index++] = baseIndex + (i + 1)*ringVertexCount + j;
				indices[index++] = baseIndex + i*ringVertexCount + j + 1;
				indices[index++] = baseIndex + (i + 1)*ringVertexCount + j + 1;
			}
		}
		int southPoleIndex = verticesCount - 1;
		baseIndex = southPoleIndex - ringVertexCount;
		for (int i = 0; i < sliceCount; i++) {
			int index = i * 3 + sliceCount * 3 + (stackCount - 2) * sliceCount * 6;
			indices[index++] = southPoleIndex;
			indices[index++] = baseIndex + i;
			indices[index++] = baseIndex + i + 1;
		}
		m_vertexBuffer = m_service.Device.CreateVertexBuffer(vertices, verticesCount);
		m_indexBuffer = m_service.Device.CreateIndexBuffer(indices, indicesCount);
		delete[] vertices;
		delete[] indices;
	}

	else if (millerType == 'f')
	{
		height = radius * 5;
		verticesCount = (stackCount + 1) * sliceCount * 2;
		indicesCount = 6 * stackCount * sliceCount * 2;
		VertexPosNormal* vertices = new VertexPosNormal[verticesCount];
		unsigned short* indices = new unsigned short[indicesCount];
		float y = height;
		float dy = height / stackCount;
		float dp = XM_2PI / sliceCount;
		int k = 0;
		for (int i = 0; i <= stackCount; ++i, y -= dy)
		{
			float phi = 0.0f;
			for (int j = 0; j < sliceCount; ++j, phi += dp)
			{
				float sinp, cosp;
				XMScalarSinCos(&sinp, &cosp, phi);
				vertices[k].Pos = XMFLOAT3(radius*cosp, y, radius*sinp);
				vertices[k++].Normal = XMFLOAT3(cosp, 0, sinp);
			}
		}
		y = height;
		dy = height / stackCount;
		dp = XM_2PI / sliceCount;
		for (int i = 0; i <= stackCount; ++i, y -= dy)
		{
			float phi = 0.0f;
			for (int j = 0; j < sliceCount; ++j, phi += dp)
			{
				float sinp, cosp;
				XMScalarSinCos(&sinp, &cosp, phi);
				vertices[k].Pos = XMFLOAT3(radius*cosp, y, radius*sinp);
				vertices[k++].Normal = XMFLOAT3(0, 0, 0);
			}
		}
		k = 0;
		for (int i = 0; i < stackCount * 2; ++i)
		{
			int j = 0;
			for (; j < sliceCount - 1; ++j)
			{
				indices[k++] = i*sliceCount + j;
				indices[k++] = i*sliceCount + j + 1;
				indices[k++] = (i + 1)*sliceCount + j + 1;
				indices[k++] = i*sliceCount + j;
				indices[k++] = (i + 1)*sliceCount + j + 1;
				indices[k++] = (i + 1)*sliceCount + j;
			}
			indices[k++] = i*sliceCount + j;
			indices[k++] = i*sliceCount;
			indices[k++] = (i + 1)*sliceCount;
			indices[k++] = i*sliceCount + j;
			indices[k++] = (i + 1)*sliceCount;
			indices[k++] = (i + 1)*sliceCount + j;
		}
		m_vertexBuffer = m_service.Device.CreateVertexBuffer(vertices, verticesCount);
		m_indexBuffer = m_service.Device.CreateIndexBuffer(indices, indicesCount);
		delete[] vertices;
		delete[] indices;
	}

	SetPosition(circleCenter.x, circleCenter.y, circleCenter.z);
}

void Miller::preDraw()
{
	m_service.Context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
	XMFLOAT4 color = XMFLOAT4(1.0, 0.0, 1.0f, 1.0);
	m_service.Context->UpdateSubresource(m_service.cbSurfaceColor.get(), 0, 0, &color, 0, 0);
	XMFLOAT4 colors[5];
}

void Miller::onDraw()
{
	ID3D11Buffer* b = m_vertexBuffer.get();
	m_service.Context->IASetVertexBuffers(0, 1, &b, &VB_STRIDE_WITH_NORMAL, &VB_OFFSET);
	m_service.Context->IASetIndexBuffer(m_indexBuffer.get(), DXGI_FORMAT_R16_UINT, 0);
	m_service.Context->DrawIndexed(indicesCount, 0, 0);
}

void Miller::afterDraw()
{
	m_service.Context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
	XMFLOAT4 color = XMFLOAT4(1.0, 1.0, 1.0f, 1.0);
	m_service.Context->UpdateSubresource(m_service.cbSurfaceColor.get(), 0, 0, &color, 0, 0);
	XMFLOAT4 colors[5];
}

void Miller::setLineTopology()
{
}

void Miller::setTriangleTopology()
{
}