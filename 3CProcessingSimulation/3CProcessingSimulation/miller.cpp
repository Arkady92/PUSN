#include "miller.h"

using namespace gk2;


Miller::Miller(Service &service, int _millerSize) : ModelClass(service)
{
	//m_Type = ModelType::CursorType;
	millerSize = _millerSize;
	ModelClass::Initialize();
	Miller::Initialize();
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
	XMVECTOR len = (pos - miller_center) * (pos - miller_center);
	float abs = 0;
	float l = XMVectorGetX(XMVector3Length(len));
	float diff = l - m_radius * m_radius;
	if (l < (m_radius * m_radius - abs) || XMVectorGetY(pos) > XMVectorGetY(miller_center))
	{
		calculateOffset(pos, miller_center);
		return true;
	}
	return false;
}

void Miller::calculateOffset(XMVECTOR& pos, XMVECTOR& miller_center)
{
	XMVECTOR o, cen, l, d;
	float r = m_radius;
	//XMVECTOR miller_center = XMVectorSet(circleCenter.x, circleCenter.y, circleCenter.z, 1);
	o = pos;
	cen = miller_center;
	l = XMVectorSet(0, -1, 0, 1);

	//double A = vx * vx + vy * vy + vz * vz;
	double A = XMVectorGetX(XMVector3LengthSq(l));
	//double B = 2.0 * (px * vx + py * vy + pz * vz - vx * cx - vy * cy - vz * cz);
	XMVECTOR B_vec = 2 * (l * (o - cen));
	double B = XMVectorGetX(B_vec) + XMVectorGetY(B_vec) + XMVectorGetZ(B_vec);
	XMVECTOR C_vec = -2 * o * cen;
	float c_val = XMVectorGetX(C_vec) + XMVectorGetY(C_vec) + XMVectorGetZ(C_vec);
	double C = XMVectorGetX(XMVector3LengthSq(o)) + c_val + XMVectorGetX(XMVector3LengthSq(cen)) - m_radius * m_radius;;

	//double C = px * px - 2 * px * cx + cx * cx + py * py - 2 * py * cy + cy * cy +
		//pz * pz - 2 * pz * cz + cz * cz - m_radius * m_radius;

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
	float height = millerSize / 100;
	float radius = m_radius;
	float topRadius = 1.0f;
	float bottomRadius = 1.0f;
	int sliceCount = 30;
	int stackCount = 30;
	float stackHeight = height / (float)stackCount;
	float radiusStep = (topRadius - bottomRadius) / stackCount;
	int ringCount = stackCount + 1;
	verticesCount = (stackCount - 1) * (sliceCount + 1) + 2;
	indicesCount = sliceCount * 3 + sliceCount * 3 + (stackCount - 2) * sliceCount * 6;
	VertexPosNormal* vertices = new VertexPosNormal[verticesCount];
	unsigned short* indices = new unsigned short[indicesCount];
	//vertices[0].Pos = XMFLOAT3(circleCenter.x, circleCenter.y + radius, circleCenter.z);
	vertices[0].Pos = XMFLOAT3(0, radius, 0);
	vertices[0].Normal = XMFLOAT3(0, 0, 1);
	//ret.Vertices.Add(new Vertex(0, radius, 0, 0, 1, 0, 1, 0, 0, 0, 0));
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

			//var t = new Vector3(-radius*MathF.Sin(phi)*MathF.Sin(theta), 0, radius*MathF.Sin(phi)*MathF.Cos(theta));
			//t.Normalize();
			//var n = p;
			//n.Normalize();
			//var uv = new Vector2(theta / (MathF.PI * 2), phi / MathF.PI);
			int index = (i - 1) * (sliceCount + 1) + j + 1;
			//p = XMVector3Transform(p, XMMatrixTranslation(circleCenter.x, circleCenter.y, circleCenter.z));
			vertices[index].Pos = XMFLOAT3(XMVectorGetX(p), XMVectorGetY(p), XMVectorGetZ(p));
			vertices[index].Normal = XMFLOAT3(0, 0, 1);
			//ret.Vertices.Add(new Vertex(p, n, t, uv));
		}
	}
	/*vertices[verticesCount - 1].Pos = XMFLOAT3(circleCenter.x, circleCenter.y -radius, circleCenter.z);*/
	vertices[verticesCount - 1].Pos = XMFLOAT3(0, 0 - radius, 0);
	vertices[verticesCount - 1].Normal = XMFLOAT3(0, 0, 1);

	//ret.Vertices.Add(new Vertex(0, -radius, 0, 0, -1, 0, 1, 0, 0, 0, 1));


	for (int i = 1; i <= sliceCount; i++) {
		int index = i * 3;
		indices[index++] = 0;
		indices[index++] = i + 1;
		indices[index++] = i;
		//ret.Indices.Add(0);
		//ret.Indices.Add(i + 1);
		//ret.Indices.Add(i);
	}
	int baseIndex = 1;
	int ringVertexCount = sliceCount + 1;
	for (int i = 0; i < stackCount - 2; i++) {
		for (int j = 0; j < sliceCount; j++) {
			int index = (i * sliceCount + j) * 6 + sliceCount * 3;
			indices[index++] = baseIndex + i*ringVertexCount + j;
			indices[index++] = baseIndex + i*ringVertexCount + j + 1;
			indices[index++] = baseIndex + (i + 1)*ringVertexCount + j;
			//ret.Indices.Add(baseIndex + i*ringVertexCount + j);
			//ret.Indices.Add(baseIndex + i*ringVertexCount + j + 1);
			//ret.Indices.Add(baseIndex + (i + 1)*ringVertexCount + j);

			indices[index++] = baseIndex + (i + 1)*ringVertexCount + j;
			indices[index++] = baseIndex + i*ringVertexCount + j + 1;
			indices[index++] = baseIndex + (i + 1)*ringVertexCount + j + 1;

			//ret.Indices.Add(baseIndex + (i + 1)*ringVertexCount + j);
			//ret.Indices.Add(baseIndex + i*ringVertexCount + j + 1);
			//ret.Indices.Add(baseIndex + (i + 1)*ringVertexCount + j + 1);
		}
	}
	int southPoleIndex = verticesCount - 1;
	baseIndex = southPoleIndex - ringVertexCount;
	for (int i = 0; i < sliceCount; i++) {
		int index = i * 3 + sliceCount * 3 + (stackCount - 2) * sliceCount * 6;
		indices[index++] = southPoleIndex;
		indices[index++] = baseIndex + i;
		indices[index++] = baseIndex + i + 1;

		//ret.Indices.Add(southPoleIndex);
		//ret.Indices.Add(baseIndex + i);
		//ret.Indices.Add(baseIndex + i + 1);
	}
	m_vertexBuffer = m_service.Device.CreateVertexBuffer(vertices, verticesCount);
	m_indexBuffer = m_service.Device.CreateIndexBuffer(indices, indicesCount);
	delete[] vertices;
	delete[] indices;

	SetPosition(circleCenter.x, circleCenter.y, circleCenter.z);
}


//void Miller::InitializeCyllinder()
//{
//	// Set the number of vertices in the vertex array.
//	m_vertexCount = 4;
//	//ilosc punktow przyblizaj¹cych okrag
//	//verticesCount = 120;
//	//indicesCount = verticesCount * 2;
//	//VertexPosNormal* vertices = new VertexPosNormal[verticesCount];
//	//unsigned short* indices = new unsigned short[indicesCount];
//
//	float height = 1.0f;
//	float topRadius = 1.0f;
//	float bottomRadius = 1.0f;
//	int sliceCount = 20;
//	int stackCount = 20;
//	float stackHeight = height / (float)stackCount;
//	float radiusStep = (topRadius - bottomRadius) / stackCount;
//	int ringCount = stackCount + 1;
//
//	verticesCount = ringCount * (sliceCount + 1);
//	indicesCount = stackCount * sliceCount * 6;
//	VertexPosNormal* vertices = new VertexPosNormal[verticesCount];
//	unsigned short* indices = new unsigned short[indicesCount];
//
//
//	//k¹t o jaki bêd¹ obracane punkty
//	float delta = XM_2PI / (float)verticesCount;
//
//	//wyznaczanie punktów
//	//for (int t = 0; t < verticesCount; t++)
//	//{
//
//	//	XMVECTOR pos = XMLoadFloat3(
//	//		&XMFLOAT3(
//	//		circleRadius * cos(t * delta),
//	//		circleRadius * sin(t * delta),
//	//		0));
//
//	//	//pos = XMVector3Transform(pos, XMMatrixRotationY(XM_PIDIV2));
//	//	//pos = XMVector3Transform(pos, XMMatrixRotationZ(XM_PI / 6.0f));
//	//	pos = XMVector3Transform(pos, XMMatrixTranslation(circleCenter.x, circleCenter.y, circleCenter.z));
//
//	//	vertices[t].Pos = XMFLOAT3(XMVectorGetX(pos), XMVectorGetY(pos), XMVectorGetZ(pos));
//	//	vertices[t].Normal = XMFLOAT3(sqrt(3) / 2.0f, 0.5f, 0.0f);
//	//}
//
//
//
//	for (int i = 0; i < ringCount; i++) {
//		float y = -0.5f*height + i*stackHeight;
//		float r = bottomRadius + i*radiusStep;
//		float dTheta = XM_2PI / (float)sliceCount;
//		for (int j = 0; j <= sliceCount; j++) {
//
//			float c = cos(j*dTheta);
//			float s = sin(j*dTheta);
//
//			XMVECTOR v = XMVectorSet(r*c, y, r*s, 1);
//			XMVECTOR uv = XMVectorSet((float)j / (float)sliceCount, 1.0f - (float)i / (float)stackCount, 0, 1);
//			XMVECTOR t = XMVectorSet(-s, 0.0f, c, 1);
//
//			float dr = bottomRadius - topRadius;
//			XMVECTOR bitangent = XMVectorSet(dr*c, -height, dr*s, 1);
//
//			XMVECTOR n = XMVector3Normalize( XMVector3Cross(t, bitangent));
//			int index = i * (sliceCount + 1) + j;
//
//			vertices[index].Pos = XMFLOAT3(XMVectorGetX(v), XMVectorGetY(v), XMVectorGetZ(v));
//			vertices[index].Normal = XMFLOAT3(0, 0, 1);
//			//ret.Vertices.Add(new Vertex(v, n, t, uv));
//		}
//	}
//
//	int ringVertexCount = sliceCount + 1;
//	for (int i = 0; i < stackCount; i++) {
//		for (int j = 0; j < sliceCount; j++) {
//			int index = (i * sliceCount + j) * 6;
//			indices[index++] = i*ringVertexCount + j;
//			indices[index++] = (i + 1)*ringVertexCount + j;
//			indices[index++] = (i + 1)*ringVertexCount + j + 1;
//			//ret.Indices.Add(i*ringVertexCount + j);
//			//ret.Indices.Add((i + 1)*ringVertexCount + j);
//			//ret.Indices.Add((i + 1)*ringVertexCount + j + 1);
//
//			indices[index++] = i*ringVertexCount + j;
//			indices[index++] = (i + 1)*ringVertexCount + j + 1;
//			indices[index++] = i*ringVertexCount + j + 1;
//			//ret.Indices.Add(i*ringVertexCount + j);
//			//ret.Indices.Add((i + 1)*ringVertexCount + j + 1);
//			//ret.Indices.Add(i*ringVertexCount + j + 1);
//		}
//	}
//
//
//
//
//
//
//
//	m_vertexBuffer = m_service.Device.CreateVertexBuffer(vertices, verticesCount);
//	//int counter = 0;
//	//for (int i = 0; i < indicesCount - 2; i += 2)
//	//{
//	//	indices[i] = counter++;
//	//	indices[i + 1] = counter;
//	//}
//	//indices[indicesCount - 2] = counter;
//	//indices[indicesCount - 1] = 0;
//	m_indexBuffer = m_service.Device.CreateIndexBuffer(indices, indicesCount);
//	delete[] vertices;
//	delete[] indices;
//}


//void  CreateSphere(float radius, int sliceCount, int stackCount) {
//	//var ret = new MeshData();
//	ret.Vertices.Add(new Vertex(0, radius, 0, 0, 1, 0, 1, 0, 0, 0, 0));
//	var phiStep = MathF.PI / stackCount;
//	var thetaStep = 2.0f*MathF.PI / sliceCount;
//
//	for (int i = 1; i <= stackCount - 1; i++) {
//		var phi = i*phiStep;
//		for (int j = 0; j <= sliceCount; j++) {
//			var theta = j*thetaStep;
//			var p = new Vector3(
//				(radius*MathF.Sin(phi)*MathF.Cos(theta)),
//				(radius*MathF.Cos(phi)),
//				(radius* MathF.Sin(phi)*MathF.Sin(theta))
//				);
//
//			var t = new Vector3(-radius*MathF.Sin(phi)*MathF.Sin(theta), 0, radius*MathF.Sin(phi)*MathF.Cos(theta));
//			t.Normalize();
//			var n = p;
//			n.Normalize();
//			var uv = new Vector2(theta / (MathF.PI * 2), phi / MathF.PI);
//			ret.Vertices.Add(new Vertex(p, n, t, uv));
//		}
//	}
//	ret.Vertices.Add(new Vertex(0, -radius, 0, 0, -1, 0, 1, 0, 0, 0, 1));
//
//
//	for (int i = 1; i <= sliceCount; i++) {
//		ret.Indices.Add(0);
//		ret.Indices.Add(i + 1);
//		ret.Indices.Add(i);
//	}
//	var baseIndex = 1;
//	var ringVertexCount = sliceCount + 1;
//	for (int i = 0; i < stackCount - 2; i++) {
//		for (int j = 0; j < sliceCount; j++) {
//			ret.Indices.Add(baseIndex + i*ringVertexCount + j);
//			ret.Indices.Add(baseIndex + i*ringVertexCount + j + 1);
//			ret.Indices.Add(baseIndex + (i + 1)*ringVertexCount + j);
//
//			ret.Indices.Add(baseIndex + (i + 1)*ringVertexCount + j);
//			ret.Indices.Add(baseIndex + i*ringVertexCount + j + 1);
//			ret.Indices.Add(baseIndex + (i + 1)*ringVertexCount + j + 1);
//		}
//	}
//	var southPoleIndex = ret.Vertices.Count - 1;
//	baseIndex = southPoleIndex - ringVertexCount;
//	for (int i = 0; i < sliceCount; i++) {
//		ret.Indices.Add(southPoleIndex);
//		ret.Indices.Add(baseIndex + i);
//		ret.Indices.Add(baseIndex + i + 1);
//	}
//	return ret;
//}

void Miller::preDraw()
{
	m_service.Context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_LINELIST);
	XMFLOAT4 color = XMFLOAT4(1.0, 1.0, 0.0f, 1.0);
	m_service.Context->UpdateSubresource(m_service.cbSurfaceColor.get(), 0, 0, &color, 0, 0);
	XMFLOAT4 colors[5];
	ZeroMemory(colors, sizeof(XMFLOAT4) * 5);
	colors[0] = XMFLOAT4(0.2f, 0.2f, 0.2f, 1.0f); //ambient color
	colors[1] = XMFLOAT4(0.0f, 1.0f, 0.0f, 1.0f); //surface [ka, kd, ks, m]
	colors[2] = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f); //light0 color
	//colors[3] = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f); //light0 color
	//colors[4] = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f); //light0 color
}

void Miller::onDraw()
{
	ID3D11Buffer* b = m_vertexBuffer.get();
	m_service.Context->IASetVertexBuffers(0, 1, &b, &VB_STRIDE_WITH_NORMAL, &VB_OFFSET);
	m_service.Context->IASetIndexBuffer(m_indexBuffer.get(), DXGI_FORMAT_R16_UINT, 0);
	m_service.Context->DrawIndexed(indicesCount, 0, 0);
	//XMFLOAT4 color = XMFLOAT4(0.0, 1.0, 0.0f, 1.0);
	//m_service.Context->UpdateSubresource(m_service.cbSurfaceColor.get(), 0, 0, &color, 0, 0);
	//b = m_vertexBuffer_SquareFirst.get();
	//m_service.Context->IASetVertexBuffers(0, 1, &b, &VB_STRIDE_WITH_NORMAL, &VB_OFFSET);
	//m_service.Context->IASetIndexBuffer(m_indexBuffer_SquareFirst.get(), DXGI_FORMAT_R16_UINT, 0);
	//m_service.Context->DrawIndexed(indicesCount, 0, 0);
	//color = XMFLOAT4(0.0, 0.0, 1.0f, 1.0);
	//m_service.Context->UpdateSubresource(m_service.cbSurfaceColor.get(), 0, 0, &color, 0, 0);
	//b = m_vertexBuffer_SquareSecond.get();
	//m_service.Context->IASetVertexBuffers(0, 1, &b, &VB_STRIDE_WITH_NORMAL, &VB_OFFSET);
	//m_service.Context->IASetIndexBuffer(m_indexBuffer_SquareSecond.get(), DXGI_FORMAT_R16_UINT, 0);
	//m_service.Context->DrawIndexed(indicesCount, 0, 0);
}

void Miller::afterDraw()
{
	m_service.Context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
	XMFLOAT4 color = XMFLOAT4(1.0, 1.0, 1.0f, 1.0);
	m_service.Context->UpdateSubresource(m_service.cbSurfaceColor.get(), 0, 0, &color, 0, 0);
	XMFLOAT4 colors[5];
	ZeroMemory(colors, sizeof(XMFLOAT4) * 5);
	colors[0] = XMFLOAT4(0.2f, 0.2f, 0.2f, 1.0f); //ambient color
	colors[1] = XMFLOAT4(0.0f, 0.0f, 1.0f, 1.0f); //surface [ka, kd, ks, m]
	colors[2] = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f); //light0 color
	//colors[3] = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f); //light0 color
	//colors[4] = XMFLOAT4(1.0f, 1.0f, 1.0f, 1.0f); //light0 color
}

void Miller::setLineTopology()
{
}

void Miller::setTriangleTopology()
{
}