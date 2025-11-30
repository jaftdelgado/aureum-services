import { Asset } from '@entities/asset.entity';

export class TeamAssetDetailDto {
  teamAssetId: number;
  publicId: string;
  teamId: string;
  assetId: string;
  currentPrice: number;
  asset: Asset;
}
