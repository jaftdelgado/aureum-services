import { Entity, PrimaryGeneratedColumn, Column } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';

@Entity({ name: 'movements' })
export class Movement {
  @ApiProperty({ description: 'ID interno del movimiento', example: 1 })
  @PrimaryGeneratedColumn({ name: 'movementid' })
  movementid: number;

  @ApiProperty({
    description: 'ID público único del movimiento',
    example: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
  })
  @Column({
    type: 'uuid',
    unique: true,
    default: () => 'uuid_generate_v4()',
    name: 'publicid',
  })
  publicid: string;

  @ApiProperty({
    description: 'ID del usuario que realizó el movimiento',
    example: '9a8b7c6d-1234-5678-9012-abcdefabcdef',
  })
  @Column({ type: 'uuid', name: 'userid' })
  userid: string;

  @ApiProperty({
    description: 'ID del asset relacionado',
    example: 'ebdd94b5-c262-4747-8fb3-d8310fc148a3',
  })
  @Column({ type: 'uuid', name: 'assetid' })
  assetid: string;

  @ApiProperty({
    description: 'Cantidad comprada o vendida',
    example: 10.5,
    type: Number,
  })
  @Column({ type: 'numeric', precision: 18, scale: 6, name: 'quantity' })
  quantity: number;

  @ApiProperty({
    description: 'Fecha de creación del movimiento',
    example: '2025-12-07T12:34:56Z',
  })
  @Column({
    type: 'timestamp',
    default: () => 'CURRENT_TIMESTAMP',
    name: 'createddate',
  })
  createddate: Date;

  @ApiProperty({
    description: 'ID del equipo asociado, si aplica',
    example: '7c9b9546-eedc-4d4a-9ae1-961a258f388c',
    required: false,
  })
  @Column({ type: 'uuid', nullable: true, name: 'teamid' })
  teamid: string;
}
