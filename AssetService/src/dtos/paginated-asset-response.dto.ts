import { ApiProperty } from '@nestjs/swagger';
import { AssetResponseDto } from './asset-response.dto';

export class PaginatedAssetResponseDto {
  @ApiProperty({ type: [AssetResponseDto] })
  data: AssetResponseDto[];

  @ApiProperty()
  meta: {
    totalItems: number;
    itemCount: number;
    itemsPerPage: number;
    totalPages: number;
    currentPage: number;
  };
}
