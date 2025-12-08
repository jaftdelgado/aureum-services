import { Controller, Get, Body, Param, Query } from '@nestjs/common';
import { AssetService } from '@services/asset.service';
import { GetAssetsDto } from '@dtos/get-assets.dto';
import { Asset } from '@entities/asset.entity';
import { PaginatedResult } from '@utils/pagination.util';
import {
  ApiTags,
  ApiOperation,
  ApiResponse,
  ApiParam,
  ApiQuery,
  ApiBearerAuth,
} from '@nestjs/swagger';

@ApiTags('Assets')
@ApiBearerAuth('access-token')
@Controller('assets')
export class AssetController {
  constructor(private readonly assetService: AssetService) {}

  @Get()
  @ApiOperation({ summary: 'Obtener listado paginado de assets' })
  @ApiQuery({ name: 'page', required: false, description: 'Número de página' })
  @ApiQuery({
    name: 'limit',
    required: false,
    description: 'Cantidad de items por página',
  })
  @ApiQuery({
    name: 'search',
    required: false,
    description: 'Filtro por nombre o símbolo',
  })
  @ApiResponse({
    status: 200,
    description: 'Lista paginada de assets',
  })
  getAssets(
    @Query() getAssetsDto: GetAssetsDto,
  ): Promise<PaginatedResult<Asset>> {
    return this.assetService.getAssets(getAssetsDto);
  }

  @Get(':publicId')
  @ApiOperation({ summary: 'Obtener un asset por su publicId' })
  @ApiParam({ name: 'publicId', description: 'ID público del asset' })
  @ApiResponse({ status: 200, description: 'Asset encontrado', type: Asset })
  @ApiResponse({ status: 404, description: 'Asset no encontrado' })
  findOne(@Param('publicId') publicId: string): Promise<Asset> {
    return this.assetService.findOneByPublicId(publicId);
  }
}
