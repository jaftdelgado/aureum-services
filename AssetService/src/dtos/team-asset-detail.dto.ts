// src/dtos/team-asset-detail.dto.ts
import { ApiProperty } from '@nestjs/swagger';
import { AssetResponseDto } from './asset-response.dto';

export class TeamAssetDetailDto {
  @ApiProperty({ description: 'ID interno del TeamAsset', example: 5 })
  teamAssetId: number;

  @ApiProperty({
    description: 'UUID público del TeamAsset',
    example: '9667e214-55a6-4e3e-8238-b4d7eef0b7f0',
  })
  publicId: string;

  @ApiProperty({
    description: 'ID del equipo asociado',
    example: '7c9b9546-eedc-4d4a-9ae1-961a258f388c',
  })
  teamId: string;

  @ApiProperty({
    description: 'ID del asset asociado',
    example: '774194f5-62ca-48e0-b067-2e61cad504b7',
  })
  assetId: string;

  @ApiProperty({ description: 'Precio actual del asset', example: 62.3 })
  currentPrice: number;

  @ApiProperty({
    description: 'Información completa del asset',
    type: () => AssetResponseDto,
  })
  asset: AssetResponseDto;
}
