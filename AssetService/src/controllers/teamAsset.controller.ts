import {
  Controller,
  Get,
  Param,
  Post,
  Body,
  HttpCode,
  HttpStatus,
} from '@nestjs/common';
import { TeamAssetService } from '@services/teamAsset.service';
import { SyncTeamAssetsDto } from '@dtos/sync-team-assets.dto';
import { TeamAssetDetailDto } from '@dtos/team-asset-detail.dto';
import {
  ApiTags,
  ApiOperation,
  ApiResponse,
  ApiParam,
  ApiBody,
  ApiBearerAuth,
} from '@nestjs/swagger';

@ApiTags('TeamAssets')
@ApiBearerAuth('access-token')
@Controller('team-assets')
export class TeamAssetController {
  constructor(private readonly teamAssetService: TeamAssetService) {}

  @Get('team/:teamId')
  @ApiOperation({ summary: 'Obtener todos los assets de un equipo' })
  @ApiParam({ name: 'teamId', description: 'ID del equipo' })
  @ApiResponse({
    status: 200,
    description: 'Lista de TeamAssets con información del Asset',
    type: [TeamAssetDetailDto],
  })
  async getByTeam(@Param('teamId') teamId: string) {
    return this.teamAssetService.findAllByTeamId(teamId);
  }

  @Get(':publicId')
  @ApiOperation({ summary: 'Obtener un TeamAsset por su publicId' })
  @ApiParam({ name: 'publicId', description: 'ID público del TeamAsset' })
  @ApiResponse({
    status: 200,
    description: 'TeamAsset encontrado con información del Asset',
    type: TeamAssetDetailDto,
  })
  @ApiResponse({ status: 404, description: 'TeamAsset no encontrado' })
  async getByPublicId(@Param('publicId') publicId: string) {
    return this.teamAssetService.findOneByPublicId(publicId);
  }

  @Post('sync')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({
    summary: 'Sincronizar los assets de un equipo',
    description:
      'Agrega o elimina assets del equipo según los IDs seleccionados. No se pueden eliminar assets con movimientos registrados.',
  })
  @ApiBody({ type: SyncTeamAssetsDto })
  @ApiResponse({
    status: 200,
    description: 'Lista de TeamAssets actualizada con información del Asset',
    type: [TeamAssetDetailDto],
  })
  @ApiResponse({
    status: 400,
    description: 'No se puede eliminar un asset con movimientos',
  })
  async syncTeamAssets(@Body() dto: SyncTeamAssetsDto) {
    const { teamId, selectedAssetIds } = dto;
    return this.teamAssetService.syncTeamAssets(teamId, selectedAssetIds);
  }
}
