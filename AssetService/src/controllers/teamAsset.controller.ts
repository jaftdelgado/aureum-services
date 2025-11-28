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
import { CreateTeamAssetDto } from '@dtos/create-team-asset.dto';

@Controller('team-assets')
export class TeamAssetController {
  constructor(private readonly teamAssetService: TeamAssetService) {}

  @Get('team/:teamId')
  async getByTeam(@Param('teamId') teamId: string) {
    return this.teamAssetService.findAllByTeamId(teamId);
  }

  @Get(':publicId')
  async getByPublicId(@Param('publicId') publicId: string) {
    return this.teamAssetService.findOneByPublicId(publicId);
  }

  @Post()
  @HttpCode(HttpStatus.CREATED)
  async createAssociation(@Body() dto: CreateTeamAssetDto) {
    const { teamId, assetPublicId } = dto;
    return this.teamAssetService.createAssociation(teamId, assetPublicId);
  }
}
