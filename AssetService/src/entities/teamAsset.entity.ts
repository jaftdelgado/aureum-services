import { Entity, PrimaryGeneratedColumn, Column } from 'typeorm';

@Entity('teamassets')
export class TeamAsset {
  @PrimaryGeneratedColumn({ name: 'teamassetid' })
  teamAssetId: number;

  @Column({
    name: 'publicid',
    type: 'uuid',
    unique: true,
    default: () => 'uuid_generate_v4()',
  })
  publicId: string;

  @Column({ name: 'teamid', type: 'uuid' })
  teamId: string;

  @Column({ name: 'assetid', type: 'uuid' })
  assetId: string;

  @Column({ name: 'currentprice', type: 'double precision' })
  currentPrice: number;
}
