#pragma once
#include "service.h"

using namespace gk2;

namespace gk2{
	Service::Service()
	{
	}

	Service::Service(const Service& other)
	{
		this->Context = other.Context;
		this->Device = other.Device;
		this->cbSurfaceColor = other.cbSurfaceColor;
		this->Scene = other.Scene;
	}

	Service::~Service()
	{
	}
}
