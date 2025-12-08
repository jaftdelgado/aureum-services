import {
  IsString,
  IsUUID,
  ArrayUnique,
  ArrayNotEmpty,
  IsArray,
} from 'class-validator';
import { ApiProperty } from '@nestjs/swagger';

export class SyncTeamAssetsDto {
  @ApiProperty({
    description: 'ID del equipo',
    type: String,
    example: '7c9b5546-eedc-4d4a-9ae1-9617958f8c',
  })
  @IsString()
  teamId: string;

  @ApiProperty({
    description: 'Lista de IDs de assets a sincronizar con el equipo',
    type: [String],
    example: [
      '77m994f5-62ca-48e0-b067-2e61cad504b7',
      '7p3390a6-10b7-4eb6-ba53-ff96183befd6',
    ],
  })
  @IsArray()
  @ArrayNotEmpty()
  @ArrayUnique()
  @IsUUID('all', { each: true })
  selectedAssetIds: string[];
}
