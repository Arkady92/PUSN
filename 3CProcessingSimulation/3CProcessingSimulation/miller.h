#pragma once

#include <d3d11.h>
#include <d3dx10math.h>
#include "modelclass.h"

using namespace std;

namespace gk2{
	class Miller : public ModelClass{
	public:
		Miller(Service &service, int _millerSize = 16, char _millerType = 'k');
		Miller(std::shared_ptr<ID3D11DeviceContext>);
		Miller(const Miller&);
		~Miller();
		virtual void Initialize();
		virtual void preDraw();
		virtual void onDraw();
		virtual void afterDraw();
		bool CheckIfPointIsInside(XMVECTOR& pos);
		int millerSize;
		char millerType;
	private:
		virtual void setTriangleTopology();
		virtual void setLineTopology();
		void calculateOffset(XMVECTOR& pos, XMVECTOR& miller_center);
		int verticesCount;
		int indicesCount;
		//dane kuli
		XMFLOAT3 circleCenter = XMFLOAT3(0.0f, 0.5f, 0.0f);
		float m_radius;
	};
}