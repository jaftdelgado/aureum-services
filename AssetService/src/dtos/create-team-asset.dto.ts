import { IsUUID, IsNotEmpty } from 'class-validator';

export class CreateTeamAssetDto {
  @IsUUID()
  @IsNotEmpty()
  teamId: string;

  @IsUUID()
  @IsNotEmpty()
  assetPublicId: string;
}
