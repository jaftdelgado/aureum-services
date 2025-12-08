import { Entity, PrimaryGeneratedColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity('teamassets')
export class TeamAsset {
  @ApiProperty({ description: 'ID interno del TeamAsset', example: 1 })
  @PrimaryGeneratedColumn({ name: 'teamassetid' })
  teamAssetId: number;

  @ApiProperty({
    description: 'ID público único del TeamAsset',
    example: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
  })
  @Column({
    name: 'publicid',
    type: 'uuid',
    unique: true,
    default: () => 'uuid_generate_v4()',
  })
  publicId: string;

  @ApiProperty({
    description: 'ID del equipo al que pertenece el asset',
    example: '7c9b9546-eedc-4d4a-9ae1-961a258f388c',
  })
  @Column({ name: 'teamid', type: 'uuid' })
  teamId: string;

  @ApiProperty({
    description: 'ID del asset asociado',
    example: 'ebdd94b5-c262-4747-8fb3-d8310fc148a3',
  })
  @Column({ name: 'assetid', type: 'uuid' })
  assetId: string;

  @ApiProperty({
    description: 'Precio actual del asset al asignarlo al equipo',
    example: 125.5,
  })
  @Column({ name: 'currentprice', type: 'double precision' })
  currentPrice: number;
}
